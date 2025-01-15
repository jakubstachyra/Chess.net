﻿    using ChessGame;
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
    using System;

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

            var result = await AddGameToRepositoryAsync(userIdPlayer1, userIdPlayer1);
            int gameId = result.gameId;
            lock (_lock)
                { 
 
                   _games.GetOrAdd(gameId, _ =>
                    {
                        var game = new ChessGame.GameMechanics.Game(gameId);
                        game.StartGame(gameId);
                        // --- Dodajemy Stockfisha ---
                        string stockfishPath = "../../external/engines/stockfish-windows-x86-64-avx2.exe";
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
                    return gameId;
                }
            }

            public async Task<int> InitializeGameWithPlayer(string userIdPlayer1 = "guest", string userIdPlayer2 = "guest")
            {
                var result = await AddGameToRepositoryAsync(userIdPlayer1, userIdPlayer2);
            int gameId = result.gameId;
            Console.WriteLine($"utworzylem gre indeks: {gameId}");
                lock (_lock)
                {
                    _games.GetOrAdd(gameId, _ =>
                    {
                        var game = new ChessGame.GameMechanics.Game(gameId);
                        game.StartGame(gameId);
                        _gameAlgorithms[gameId] = new Algorithms(2);

                        _gameUserAssociations[gameId] = new Dictionary<int, string>
                {
                    { 1, userIdPlayer1 },
                    { 2, userIdPlayer2 }
                };

                        return game;
                    });

                    Console.WriteLine($"Game initialized with ID: {gameId} for users: {userIdPlayer1} and {userIdPlayer2}");
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


        public List<ChessGame.GameMechanics.Move> GetAllPlayerMoves(int gameId)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    var color = game.player == 0 ? ChessGame.Color.White : ChessGame.Color.Black;
                    return game.chessBoard.GetAllPlayerMoves(color);
                }

                throw new KeyNotFoundException("Game not found.");
            }

            public void MakeSentMove(int gameId, string move)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    Position start = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[0]}{move[1]}");
                    Position end = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[2]}{move[3]}");

                    Console.Write("move: ");
                    Console.WriteLine(move);
                    game.ReceiveMove(start, end);

                    if (move.Length > 4)
                    {
                        Color playerColor = move[4] == 'w' ? Color.White : Color.Black;
                        PieceType promotedPieceType = GetPromotedPieceType(move[5]);

                        Piece promotedPiece = PieceFactory.CreatePiece(promotedPieceType, playerColor);

                        game.chessBoard.board[end.x, end.y] = promotedPiece;
                        promotedPiece.setPosition(end.x, end.y);
                    }



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
                    default:
                        throw new ArgumentException("Invalid promoted piece type");
                }
            }

            public ChessGame.GameMechanics.Move CalculateComputerMove(int gameId)
            {
                // Sprawdzamy, czy gra istnieje i Stockfish został zainicjalizowany
                if (_games.TryGetValue(gameId, out var game)
                    && _stockfishInstances.TryGetValue(gameId, out var stockfish))
                {
                    // Generujemy FEN obecnej pozycji
                    string currentFen = game.chessBoard.GenerateFEN();
                    Console.WriteLine($"Current: fen{currentFen}");
                    // Zapytaj Stockfisha o najlepszy ruch
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

                    // Wykonaj ruch na planszy
                    game.ReceiveMove(start, end);

                    // Obsługa promocji, gdy bestMoveUci ma więcej niż 4 znaki (np. "g7g8q")
                    if (bestMoveUci.Length > 4)
                    {
                        char promotionChar = bestMoveUci[4]; // 'q', 'r', 'b', 'n' (zwykle lowercase)
                        Color color = game.player == 0 ? Color.White : Color.Black;
                        PieceType pieceType = GetPromotedPieceType(promotionChar);

                        Piece promotedPiece = PieceFactory.CreatePiece(pieceType, color);
                        game.chessBoard.board[end.x, end.y] = promotedPiece;
                        promotedPiece.setPosition(end.x, end.y);
                    }

                    game.PrintBoard();

                    return new ChessGame.GameMechanics.Move(start, end);
                }

                throw new KeyNotFoundException("Game not found or Stockfish not initialized.");
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
            await AddMovesToRepositoryAsync(gameId);
                RecycleGame(gameId);

                if (_stockfishInstances.TryRemove(gameId, out var stockfish))
                {
                    stockfish.Dispose();
                }
                return true;
            }
            public async Task<bool> GetGameState(int gameId)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    var color1 = game.player == 0 ? Color.White : Color.Black;
                    var color2 = game.player == 0 ? Color.Black : Color.White;

                    // Check for checkmate
                    if (game.chessBoard.ifCheckmate(color1))
                    {
                        // color1 is checkmated. color2 is the winner.
                        // Suppose user 1 is color1, user 2 is color2
                        var winnerUserId = _gameUserAssociations[gameId][1];
                        var loserUserId = _gameUserAssociations[gameId][2];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate"
                        );
                        return true;
                    }
                    if (game.chessBoard.ifCheckmate(color2))
                    {
                        // color2 is checkmated. color1 is the winner.
                        var winnerUserId = _gameUserAssociations[gameId][2];
                        var loserUserId = _gameUserAssociations[gameId][1];

                        await EndGameAsync(
                            gameId: gameId,
                            winner: winnerUserId,
                            loser: loserUserId,
                            reason: "Checkmate"
                        );
                        return true;
                    }

                    // Check for time-out
                    if (game.chessBoard.isWhiteTimerOver || game.chessBoard.isBlackTimerOver)
                    {
                        // For example, if White's time is over, then Black is the winner
                        var winnerUserId = game.chessBoard.isWhiteTimerOver
                            ? _gameUserAssociations[gameId][2]
                            : _gameUserAssociations[gameId][1];
                        var loserUserId = game.chessBoard.isWhiteTimerOver
                            ? _gameUserAssociations[gameId][1]
                            : _gameUserAssociations[gameId][2];

                        await EndGameAsync(
                            gameId: gameId,
                            loser: loserUserId,
                            winner: winnerUserId, ///
                            reason: "By time" /// DO poprawy
                        );
                        return true;
                    }

                    // If neither is checkmated nor time-out, the game continues
                    return false;
                }

                throw new KeyNotFoundException("Game not found.");
            }

            private async Task EndGameAsync(int gameId, string winner, string loser, string reason)
            {
                // 1. Nie serializuj sam, tylko wyślij obiekt anonimowy
                await _hubContext.Clients.Group(gameId.ToString()).SendAsync("GameOver", new
                {
                    GameId = gameId,
                    Winner = winner,
                    Loser = loser,
                    Reason = reason
                });

                // 2. Zapis do bazy, recycling itd.
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
                var winnerUserId = _gameUserAssociations[gameId][winnerSide];
                var loserUserId = userId;  // The resigning side

                await _hubContext.Clients.Client(winnerUserId).SendAsync("GameOver");
                await _hubContext.Clients.Client(loserUserId).SendAsync("GameOver");

                await EndGameAsync(
                    gameId: gameId,
                    winner: winnerUserId,
                    loser: loserUserId,
                    reason: "Resignation"
                );
                return true;
            }

            public async Task<(bool Success, string Message, int gameId)> AddGameToRepositoryAsync(string whitePlayerId, string blackPlayerId)
            {

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

                    GameMode gameModetest = await dataRepository.GameModeRepository.GetByIDAsync(1); //to change

                    using var transaction = await dataRepository.BeginTransactionAsync();
                    try
                    {
                        var game = new Domain.Common.Game
                        {
                            WhitePlayer = whiteUser,
                            BlackPlayer = blackUser,
                            Date = dateTime,
                            Result = "0-0",
                            GameMode = gameModetest
                        };
                        
                        await dataRepository.GameRepository.AddAsync(game);
                        await transaction.CommitAsync();

                        Console.WriteLine($"Game added successfully. {game.Id}");
                        return (true, "Game added successfully.",game.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Failed to add game");
                        return (false, $"Failed to add game: {ex.Message}",0);
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

            public bool addMoveTime(int gameId, int remainingTime)
            {
                var game = _games[gameId];
                game.moveRemaingTimes.Add(remainingTime);
                return true;
            }
            public bool setTimeIsOver(int gameId, string color)
            {
                var game = _games[gameId];

                if (color == "white") return game.chessBoard.isWhiteTimerOver = true;
                if (color == "black") return game.chessBoard.isBlackTimerOver = true;
                return false;

            }
            public string GetGameResult(int gameId)
            {
                var game = _games[gameId];
                if (game.chessBoard.ifCheckmate(Color.Black)) return "1-0";
                if (game.chessBoard.ifCheckmate(Color.White)) return "0-1";
                if (game.chessBoard.isWhiteTimerOver == true) return "0-1";
                if (game.chessBoard.isBlackTimerOver == true) return "1-1";
                return "0-0";

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
