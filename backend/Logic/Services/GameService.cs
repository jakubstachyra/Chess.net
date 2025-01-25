using ChessGame;
using ChessGame.AI;
using ChessGame.GameMechanics;
using System.Collections.Concurrent;
using Logic.Interfaces;
using ChessGame.Pieces;
using Microsoft.AspNetCore.Identity;
using Domain.Users;
using Infrastructure.Interfaces;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Logic.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Chess.net.Services
{
    public class GameService : IGameService
    {

        private readonly IHubContext<GameHub> _hubContext;
        private readonly ConcurrentDictionary<int, ChessGame.GameMechanics.Game> _games = new();
        private readonly ConcurrentDictionary<int, Algorithms> _gameAlgorithms = new();
        private readonly ConcurrentQueue<int> _availableGameIds = new ConcurrentQueue<int>();
        private readonly Dictionary<int, Dictionary<int, string>> _gameUserAssociations = new();
        private readonly object _lock = new object();
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<int, StockfishEngine> _stockfishInstances = new();

        public GameService(IServiceProvider serviceProvider, IHubContext<GameHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }
        public async Task<int> InitializeGameWithComputer(string userIdPlayer1 = "guest")
        {

            var result = await AddGameToRepositoryAsync(userIdPlayer1, userIdPlayer1, "Computer");
            int gameId = result.gameId; 
            lock (_lock)
            {

                _games.GetOrAdd(gameId, _ =>
                {
                    var game = new ChessGame.GameMechanics.Game(gameId);
                    game.StartGame(gameId);
                    // --- Dodajemy Stockfisha ---
                    string stockfishPath = "../../external/engines/stockfish-windows-x86-64-avx2.exe";
                    //string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
                    //string filePath = Path.Combine(directoryPath, "stockfish-windows-x86-64-avx2.exe");
                    var stockfishEngine = new StockfishEngine(stockfishPath);
                    _stockfishInstances[gameId] = stockfishEngine;

                    _gameUserAssociations[gameId] = new Dictionary<int, string>
                {
                    { 1, userIdPlayer1 },
                    { 2, null }
                };

                    return game;
                });

                Console.WriteLine($"Game initialized with ID: {gameId} for user: {userIdPlayer1} and the computer (Stockfish).");
                setGameMode(gameId, "Computer");
                return gameId;
            }
        }

        public async Task<int> InitializeGameWithPlayer(string mode,int timer, string userIdPlayer1 = "guest", string userIdPlayer2 = "guest")
        {
            mode = modeConverter(mode,timer);
            var result = await AddGameToRepositoryAsync(userIdPlayer1, userIdPlayer2,mode);
            int gameId = result.gameId;
            Console.WriteLine($"mode wyslany do init game {mode}");

            lock (_lock)
            {
                _games.GetOrAdd(gameId, _ =>
                {
                  
                    var game = new ChessGame.GameMechanics.Game(gameId,mode , Logic.Services.Chess960Service.RandomStartFen());

                    game.StartGame(gameId);
                    if (mode == "brain-hand")
                    {
                        // --- Dodajemy Stockfisha ---

                        string stockfishPath = "../../external/engines/stockfish-windows-x86-64-avx2.exe";
                        string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(directoryPath, "stockfish-windows-x86-64-avx2.exe");
                        var stockfishEngine = new StockfishEngine(filePath);
                        _stockfishInstances[gameId] = stockfishEngine;
                    }
                    _gameAlgorithms[gameId] = new Algorithms(2);

                    _gameUserAssociations[gameId] = new Dictionary<int, string>
                {
                    { 1, userIdPlayer1 },
                    { 2, userIdPlayer2 }
                };

                    return game;
                });

                Console.WriteLine($"Game initialized with ID: {gameId} for users: {userIdPlayer1} and {userIdPlayer2}");
                setGameMode(gameId,mode);


                return gameId;
            }
        }
        /*        private async Task<int> FindFirstAvailableGameIdAsync()
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scopedProvider = scope.ServiceProvider;
                        var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();

                        // Pobierz wszystkie gry
                        var games = await dataRepository.GameRepository.GetByConditionAsync(game => true);

                        // Znajdź największe ID lub zwróć 0, jeśli brak gier
                        var lastGameId = games.Any() ? games.Max(game => game.Id) : 0;

                        // Zwróć nowe ID
                        return lastGameId + 1;
                    }
                }
        */
        public void RecycleGame(int gameId)
        {
            lock (_lock)
            {
                // Remove the game from the _games dictionary
                if (_games.TryRemove(gameId, out var game))
                {
                    // Dispose any associated resources, such as Stockfish engine
                    if (_stockfishInstances.TryRemove(gameId, out var stockfish))
                    {
                        stockfish.Dispose();
                    }

                    // Optionally, if there's any cleanup related to the game algorithms, you can remove that as well
                    _gameAlgorithms.TryRemove(gameId, out _);

                    // Remove the user associations for the game
                    _gameUserAssociations.Remove(gameId, out _);

                    Console.WriteLine($"Game {gameId} has been recycled and removed from memory.");
                }
                else
                {
                    Console.WriteLine($"Game {gameId} not found for recycling.");
                }
            }
        }

        private int FindFirstAvailableGameId()
        {
            lock (_lock)
            {
                if (_availableGameIds.TryDequeue(out var gameId))
                {
                    return gameId;
                }

                return _games.Count + 1;
            }
        }

        public void RecycleGameId(int gameId)
        {
            lock (_lock)
            {
                _availableGameIds.Enqueue(gameId);
            }
        }

        public List<ChessGame.GameMechanics.Move> GetAllPlayerMoves(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                string currentFen = game.chessBoard.GenerateFEN();
                Console.WriteLine($"Current: fen{currentFen}");


                Console.WriteLine(game.gameMode);
               var color = game.player == 0 ? ChessGame.Color.White : ChessGame.Color.Black;
               var moves = game.chessBoard.GetAllPlayerMoves(color);
            
                if (game.gameMode == "brain-hand" && _stockfishInstances.TryGetValue(gameId, out var stockfish))
                {
                    Console.WriteLine("weszlem w stockfisha");
                    string bestMoveUci = stockfish.GetBestMoveAsync(currentFen, 1).Result;
                    char startFile = bestMoveUci[0];
                    char startRank = bestMoveUci[1];
                    string startSquare = $"{startFile}{startRank}";
                    var position = ChessGame.Utils.Converter.ChessNotationToPosition(startSquare);
                    var piece = game.chessBoard.GetPieceAt(position);
                    moves = moves.Where(move =>
                    game.chessBoard.GetPieceAt(move.from).pieceType == piece.pieceType
                ).ToList();
                }
                return moves;
            }

            throw new KeyNotFoundException("Game not found.");
        }

        public void MakeSentMove(int gameId, string move)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                Position start = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[0]}{move[1]}");
                Position end = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[2]}{move[3]}");

                Console.Write("otrzymany move: ");
                Console.WriteLine(move);

                if (move.Length == 6)
                {
                    Color playerColor = move[4] == 'w' ? Color.White : Color.Black;
                    PieceType promotedPieceType = GetPromotedPieceType(move[5]);

                    Piece promotedPiece = PieceFactory.CreatePiece(promotedPieceType, playerColor);

                    game.chessBoard.board[end.x, end.y] = promotedPiece;
                    promotedPiece.setPosition(end.x, end.y);
                }
                if (move.Length == 5)
                {
                    PieceType promotedPieceType = GetPromotedPieceType(move[4]);

                    Piece promotedPiece = PieceFactory.CreatePiece(promotedPieceType, (Color)game.player);

                    game.chessBoard.board[end.x, end.y] = promotedPiece;
                    promotedPiece.setPosition(end.x, end.y);
                }

                game.ReceiveMove(start, end);

                game.PrintBoard();
            }
            else
            {
                throw new KeyNotFoundException("Game not found.");
            }
        }

        private PieceType GetPromotedPieceType(char pieceChar)
        {
            switch (pieceChar)
            {
                case 'Q':
                    return PieceType.Queen;
                case 'R':
                    return PieceType.Rook;
                case 'B':
                    return PieceType.Bishop;
                case 'N':
                    return PieceType.Knight;
                case 'q':
                    return PieceType.Queen;
                case 'r':
                    return PieceType.Rook;
                case 'b':
                    return PieceType.Bishop;
                case 'n':
                    return PieceType.Knight;
                default:
                    throw new ArgumentException("Invalid promoted piece type");
            }
        }

        public string CalculateComputerMove(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game)
                && _stockfishInstances.TryGetValue(gameId, out var stockfish))
            {
                string currentFen = game.chessBoard.GenerateFEN();
                Console.WriteLine($"Current: fen{currentFen}");

                string bestMoveUci = stockfish.GetBestMoveAsync(currentFen, 1).Result;

                if (string.IsNullOrEmpty(bestMoveUci))
                {
                    // Może oznaczać brak ruchu (np. mat)
                    throw new InvalidOperationException("No valid moves available.");
                }

                // Rozbijamy UCI, np. "e2e4" -> start = e2, end = e4
                Position start = ChessGame.Utils.Converter.ChessNotationToPosition(bestMoveUci.Substring(0, 2));
                Position end = ChessGame.Utils.Converter.ChessNotationToPosition(bestMoveUci.Substring(2, 2));

                Console.WriteLine(start.ToString());
                Console.WriteLine(end.ToString());
                Console.WriteLine($"Stockfish move: {bestMoveUci}");

                //Teraz jest w signalR
                //game.ReceiveMove(start, end);
                // Obsługa promocji, gdy bestMoveUci ma więcej niż 4 znaki (np. "g7g8q")



                game.PrintBoard();

                //return new ChessGame.GameMechanics.Move(start, end);
                return bestMoveUci;
            }

            throw new KeyNotFoundException("Game not found or Stockfish not initialized.");
        }

        public void promoteComputerPiece(string move,int gameId)
        {
            var _move = move.ToString();
            if (_games.TryGetValue(gameId, out var game))
            { if (_move.Length > 4)
                {
                    Position start = ChessGame.Utils.Converter.ChessNotationToPosition(_move.Substring(0, 2));
                    Position end = ChessGame.Utils.Converter.ChessNotationToPosition(_move.Substring(2, 2));
                    char promotionChar = _move[4]; // 'q', 'r', 'b', 'n' (zwykle lowercase)
                    Color color = game.player == 1 ? Color.White : Color.Black; //na odwrot go gra juz widzi nastepnego gracza 
                    PieceType pieceType = GetPromotedPieceType(promotionChar);
                    Console.WriteLine(move);
                    Piece promotedPiece = PieceFactory.CreatePiece(pieceType, color);

                    game.chessBoard.board[end.x, end.y] = promotedPiece;
                    promotedPiece.setPosition(end.x, end.y);
                }
            }
        }
        public void DisposeGame(int gameId)
        {
            if (_stockfishInstances.TryRemove(gameId, out var stockfish))
            {
                stockfish.Dispose();
            }

            _games.TryRemove(gameId, out _);
        }

        public async Task<bool> GameEnded(int gameId)
        {
            await UpdateGameResultAsync(gameId);
            var game = _games[gameId];
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;

                    var rankingService= scopedProvider.GetRequiredService<IRankingService>();
                    await rankingService.CalculateDeltaAndUpdateRanking(_gameUserAssociations[gameId][1], _gameUserAssociations[gameId][2], GetGameResult(gameId), game.gameMode);
                }
            


                    
            await AddMovesToRepositoryAsync(gameId);

            if (_stockfishInstances.TryRemove(gameId, out var stockfish))
            {
                stockfish.Dispose();
            }
            RecycleGame(gameId);
            return true;
        }
        public async Task<bool> GetGameState(int gameId, bool computer = false)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                if (game.gameMode == game.modes[2])
                {
                    if(game.The_king_is_dead_long_live_the_king_Endgame(Color.White))
                    {
                        string winnerUserId = _gameUserAssociations[gameId][2];
                        var loserUserId = _gameUserAssociations[gameId][1];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate",
                            draw: false,
                            computer
                        );
                        return true;
                    }
                    if (game.The_king_is_dead_long_live_the_king_Endgame(Color.Black))
                    {
                        // color2 is checkmated. color1 is the winner.
                        var winnerUserId = _gameUserAssociations[gameId][1];
                        var loserUserId = _gameUserAssociations[gameId][2];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate",
                            draw: false,
                            computer
                        );
                        return true;
                    }
                }

                
                else
                {
                    if (game.chessBoard.ifCheckmate(Color.White))
                    {
                        string winnerUserId = _gameUserAssociations[gameId][2];
                        var loserUserId = _gameUserAssociations[gameId][1];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate",
                            draw: false,
                            computer
                        );
                        return true;
                    }
                    if (game.chessBoard.ifCheckmate(Color.Black))
                    {
                        // color2 is checkmated. color1 is the winner.
                        var winnerUserId = _gameUserAssociations[gameId][1];
                        var loserUserId = _gameUserAssociations[gameId][2];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate",
                            draw: false,
                            computer
                        );
                        return true;
                    }

                    //check for draw
                    var result = game.isDraw();
                    if (result.Item1)
                    {
                        await EndGameAsync(
                            gameId: gameId,
                            winner: "",
                            loser: "",
                            reason: result.reason,
                            draw: true,
                            computer
                        );
                        return true;
                    }
                    // Check for time-out
                    /*                if (game.chessBoard.isWhiteTimerOver || game.chessBoard.isBlackTimerOver)
                                    {
                                        var winnerUserId = game.chessBoard.isWhiteTimerOver
                                            ? _gameUserAssociations[gameId][2]
                                            : _gameUserAssociations[gameId][1];
                                        var loserUserId = game.chessBoard.isWhiteTimerOver
                                            ? _gameUserAssociations[gameId][1]
                                            : _gameUserAssociations[gameId][2];

                                        await EndGameAsync(
                                            gameId: gameId,
                                            loser: loserUserId,
                                            winner: winnerUserId,
                                            reason: "By time" 
                                        );
                                        return true;
                                    }*/
                    return false;
                }

            }
            throw new KeyNotFoundException("Game not found.");
        }

        public async Task EndGameAsync(int gameId, string winner, string loser, string reason, bool draw = false, bool computer = false)
        {
            // 1. Sprawdzenie, czy gra istnieje
            if (!_games.TryGetValue(gameId, out var game))
            {
                Console.WriteLine($"[EndGameAsync] Gra o ID {gameId} nie istnieje.");
                return;
            }

            // 2. Pobranie informacji o graczach
            if (!_gameUserAssociations.TryGetValue(gameId, out var userAssociations))

            {
                Console.WriteLine($"[EndGameAsync] Brak skojarzenia użytkowników dla gry {gameId}.");
                return;
            }

            string player1 = userAssociations.GetValueOrDefault(1);
            string player2 = userAssociations.GetValueOrDefault(2);

            // 3. Wysłanie informacji o zakończeniu gry
            if (computer)
            {
                // Gra z komputerem - wysyłamy tylko do gracza ludzkiego
                if (!string.IsNullOrEmpty(player1))
                {
                    await _hubContext.Clients.User(player1).SendAsync("GameOver", new
                    {
                        GameId = gameId,
                        Winner = winner,
                        Loser = loser,
                        Reason = reason,
                        Draw = draw
                    });

                    await _hubContext.Clients.User(player1).SendAsync("Disconnect");
                }
                else
                {
                    Console.WriteLine($"[EndGameAsync] Nie znaleziono gracza ludzkiego dla gry {gameId}.");
                }
            }
            else
            {
                // Gra dwóch graczy - wysyłamy do obu graczy
                foreach (var userId in userAssociations.Values)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _hubContext.Clients.User(userId).SendAsync("GameOver", new
                        {
                            GameId = gameId,
                            Winner = winner,
                            Loser = loser,
                            Reason = reason,
                            Draw = draw
                        });

                        await _hubContext.Clients.User(userId).SendAsync("Disconnect");
                    }
                }

            }

            // 4. Zakończenie gry w serwisie
            await GameEnded(gameId);

        }


        public async Task<bool> ResignGame(int gameId, string userId)
        {
            if (!_games.TryGetValue(gameId, out var game))
                throw new KeyNotFoundException("Game not found.");

            // Identify which color userId is playing
            var userSide = _gameUserAssociations[gameId].FirstOrDefault(kv => kv.Value == userId);
            if (userSide.Key == 0)
            {
                // Not found in either color
                throw new ArgumentException("User is not a player in this game.");
            }

            // userSide.Key = 1 or 2, meaning user is White or Black
            var winnerSide = (userSide.Key == 1) ? 2 : 1;
            setPlayerResigned(gameId,winnerSide);
            var winnerUserId = _gameUserAssociations[gameId][winnerSide];
            var loserUserId = userId;  // The resigning side

            await EndGameAsync(
                gameId: gameId,
                winner: winnerUserId,
                loser: loserUserId,
                reason: "Resignation"
            );
            return true;
        }

        public async Task<(bool Success, string Message, int gameId)> AddGameToRepositoryAsync(string whitePlayerId, string blackPlayerId, string mode)
        {
            Console.WriteLine("Otrzymany mode db:");
            Console.WriteLine(mode);
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;

                var userManager = scopedProvider.GetRequiredService<UserManager<User>>();

                var whitePlayer = whitePlayerId;
                var blackPlayer = blackPlayerId;
                //var result = GetGameResult(gameId);
                User whiteUser = await userManager.FindByIdAsync(whitePlayer);
                User blackUser = await userManager.FindByIdAsync(blackPlayer);


                DateTime dateTime = DateTime.UtcNow;

                var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();
                string gameModestrng = mode;
                var allGameModes = await dataRepository.GameModeRepository.GetAllAsync();


                GameMode gameMode = allGameModes.FirstOrDefault(gm => gm.Description.Equals(gameModestrng, StringComparison.OrdinalIgnoreCase));
                using var transaction = await dataRepository.BeginTransactionAsync();
                try
                {
                    var game = new Domain.Common.Game
                    {
                        WhitePlayer = whiteUser,
                        BlackPlayer = blackUser,
                        Date = dateTime,
                        Result = "0-0",
                        GameMode = gameMode
                    };

                    await dataRepository.GameRepository.AddAsync(game);
                    await transaction.CommitAsync();

                    Console.WriteLine($"Game added successfully. {game.Id}");
                    return (true, "Game added successfully.", game.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Failed to add game");
                    return (false, $"Failed to add game: {ex.Message}", 0);
                }

            }

        }
        public async Task<(bool Success, string Message)> AddMovesToRepositoryAsync(int gameId)
        {
            var _game = _games[gameId];

            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;



                var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();


                using var transaction = await dataRepository.BeginTransactionAsync();
                try
                {

                    var saveGameService = scopedProvider.GetRequiredService<ISaveGameService>();
                    var whiteMoves = _game.chessBoard.whiteMoves;
                    var blackMoves = _game.chessBoard.blackMoves;
                    var remaingTimes = _game.moveRemaingTimes;
                    var whiteRemainingTime = new List<int>();
                    var blackRemainingTime = new List<int>();
                    for (int i = 0; i < remaingTimes.Count; i++)
                    {
                        if (i % 2 == 0) whiteRemainingTime.Add(remaingTimes[i]);
                        else blackRemainingTime.Add(remaingTimes[i]);
                    }
                    await saveGameService.SaveMovesAsync(gameId, whiteMoves, blackMoves, whiteRemainingTime, blackRemainingTime);
                    await transaction.CommitAsync();

                    return (true, "Moves added successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Failed to add moves");
                    Console.WriteLine(ex.Message);
                    return (false, $"Failed to add moves: {ex.Message}");
                }

            }

        }
        public async Task<(bool Success, string Message)> UpdateGameResultAsync(int gameId)
        {
            var result = GetGameResult(gameId);
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();

                using var transaction = await dataRepository.BeginTransactionAsync();
                try
                {

                    var game = await dataRepository.GameRepository.GetByIDAsync(gameId);
                    if (game == null)
                    {
                        return (false, "Game not found.");
                    }

                    game.Result = result;
                    await dataRepository.GameRepository.UpdateAsync(game);
                    await transaction.CommitAsync();

                    Console.WriteLine("Game result updated successfully.");
                    return (true, "Game result updated successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Failed to update game result");
                    Console.WriteLine(ex.Message);
                    return (false, $"Failed to update game result: {ex.Message}");
                }
            }
        }

        public int WhoToMove(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                return game.player;
            }

            throw new KeyNotFoundException("Game not found.");
        }

        public string SendFen(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                return game.chessBoard.GenerateFEN();
            }

            throw new KeyNotFoundException("Game not found.");
        }

        public void ReceiveFen(int gameId, string FEN)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                game.chessBoard.LoadFEN(FEN);
            }
            else
            {
                throw new KeyNotFoundException("Game not found.");
            }
        }

        public bool setGameMode(int gameId, string mode)
        {
            var game = _games[gameId];
            game.gameMode = mode;
            return true;
        }
        public bool setTimeIsOver(int gameId, string color)
        {
            var game = _games[gameId];

            if (color == "white") return game.chessBoard.isWhiteTimerOver = true;
            if (color == "black") return game.chessBoard.isBlackTimerOver = true;
            return false;

        }

        public void setPlayerResigned(int gameId,int winerColor)
        {
            var game = _games[gameId];
            if (winerColor == 1) game.whiteResigned = true;
            if (winerColor == 2) game.blackResigned = true;

        }

        public void setPlayerDrawed(int gameId)
        {
            var game = _games[gameId];
            game.acceptedDrawOffer = true;
        }
        public string GetGameResult(int gameId)
        {
            var game = _games[gameId];
            if (game.chessBoard.ifCheckmate(Color.Black)) return "1-0";
            if (game.chessBoard.ifCheckmate(Color.White)) return "0-1";
            if (game.chessBoard.isWhiteTimerOver == true) return "0-1";
            if (game.chessBoard.isBlackTimerOver == true) return "1-0";

            if (game.blackResigned == true) return "0-1";
            if (game.whiteResigned == true) return "1-0";

            if (game.isDraw().Item1) return "1/2-1/2";
            if (game.acceptedDrawOffer) return "1/2-1/2";

            return "0-0";

        }

        public bool addMoveTime(int gameId, int remainingTime)
        {
            var game = _games[gameId];
            game.moveRemaingTimes.Add(remainingTime);
            return true;
        }

        public List<MoveHistoryEntry> GetFullMoveHistory(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                return game.MoveHistory;
            }
            throw new KeyNotFoundException("Game not found.");
        }

        public void AddMoveHistoryEntry(int gameId, string algebraicMove, string fen, int whiteTimeMs, int blackTimeMs)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                // Zwiększamy licznik ruchów
                game.MovesSoFar++;
                int moveNumber = (game.MovesSoFar + 1) / 2;

                var entry = new MoveHistoryEntry
                {
                    MoveNumber = moveNumber,
                    Fen = fen,
                    Move = algebraicMove,
                    WhiteRemainingTimeMs = whiteTimeMs,
                    BlackRemainingTimeMs = blackTimeMs
                };

                game.MoveHistory.Add(entry);
            }
            else
            {
                throw new KeyNotFoundException("Game not found.");
            }
        }

        public string modeConverter(string mode, int timer)
        {
            if (mode == "chess960") return "960";
            if (mode == "newking") return "The king is dead, long live the king!";
              if (mode == "brainhand") return "brain-hand";
            if (mode =="player")
            {
                if (timer <= 180) return "Bullet";
                if (timer <= 600) return "Blitz";
                return "Rapid";
            }
            return mode;
        }
        public bool TryGetGame(int gameId, out ChessGame.GameMechanics.Game game)
        {
            return _games.TryGetValue(gameId, out game);
        }

        //public string GetUserForGame(int gameId)
        //{
        //    if (_gameUserAssociations.TryGetValue(gameId, out var userId))
        //    {
        //        return userId;
        //    }

        //    throw new KeyNotFoundException("Game not found.");
        //}
    }
}