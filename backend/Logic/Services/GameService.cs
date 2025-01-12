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

namespace Chess.net.Services
{
    public class GameService : IGameService
    {
        private readonly ConcurrentDictionary<int, ChessGame.GameMechanics.Game> _games = new();
        private readonly ConcurrentDictionary<int, Algorithms> _gameAlgorithms = new();
        private readonly ConcurrentQueue<int> _availableGameIds = new ConcurrentQueue<int>();
        private readonly Dictionary<int, Dictionary<int, string>> _gameUserAssociations = new();
        private readonly object _lock = new object();
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<int, StockfishEngine> _stockfishInstances = new();

        public GameService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public int InitializeGameWithComputer(string userIdPlayer1 = "guest")
        {
            lock (_lock)
            {
                int newGameId = _games.Count + 1;

                _games.GetOrAdd(newGameId, _ =>
                {
                    var game = new ChessGame.GameMechanics.Game(newGameId);
                    game.StartGame(newGameId);

                    string stockfishPath = "../../external/engines/stockfish-windows-x86-64-avx2.exe";
                    var stockfishEngine = new StockfishEngine(stockfishPath);
                    _stockfishInstances[newGameId] = stockfishEngine;

                    return game;
                });

                Console.WriteLine($"Game initialized with ID: {newGameId} for user: {userIdPlayer1} and the computer.");
                return newGameId;
            }
        }


        public int InitializeGameWithPlayer(string userIdPlayer1 = "guest", string userIdPlayer2="guest")
        {
            lock (_lock)
            {
                int newGameId = FindFirstAvailableGameId();

                _games.GetOrAdd(newGameId, _ =>
                {
                    var game = new ChessGame.GameMechanics.Game(newGameId);
                    game.StartGame(newGameId);
                    _gameAlgorithms[newGameId] = new Algorithms(2);

                   
                    _gameUserAssociations[newGameId] = new Dictionary<int, string>
            {
                { 1, userIdPlayer1 },
                { 2, userIdPlayer2 }
            };

                    return game;
                });

                Console.WriteLine($"Game initialized with ID: {newGameId} for users: {userIdPlayer1} and {userIdPlayer2}");
                return newGameId;
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

        public async Task<ChessGame.GameMechanics.Move> CalculateComputerMoveAsync(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                string currentFen = game.chessBoard.GenerateFEN();

                if (_stockfishInstances.TryGetValue(gameId, out var stockfish))
                {
                    string bestMoveUci = await stockfish.GetBestMoveAsync(currentFen);

                    Position start = ChessGame.Utils.Converter.ChessNotationToPosition(bestMoveUci.Substring(0, 2));
                    Position end = ChessGame.Utils.Converter.ChessNotationToPosition(bestMoveUci.Substring(2, 2));

                    var move = new ChessGame.GameMechanics.Move(start, end);
                    game.ReceiveMove(move.from, move.to);

                    if (bestMoveUci.Length > 4)
                    {
                        char promotionChar = bestMoveUci[4];
                        Color playerColor = game.player == 0 ? Color.White : Color.Black;
                        PieceType promotedPieceType = GetPromotedPieceType(promotionChar);

                        Piece promotedPiece = PieceFactory.CreatePiece(promotedPieceType, playerColor);
                        game.chessBoard.board[end.x, end.y] = promotedPiece;
                        promotedPiece.setPosition(end.x, end.y);
                    }

                    return move;
                }

                throw new Exception("Stockfish instance not found for game ID " + gameId);
            }

            throw new KeyNotFoundException("Game not found.");
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
            Console.WriteLine("gra skonczona dodaje do db");
            await AddGameToRepositoryAsync(gameId);
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
                Console.WriteLine($"color1 :{game.chessBoard.ifCheckmate(color1)}");
                Console.WriteLine($"color2 :{game.chessBoard.ifCheckmate(color2)}");

                if (game.chessBoard.ifCheckmate(color1))
                {
                    await GameEnded(gameId);
                }
                return game.chessBoard.ifCheckmate(color1);
            }
            throw new KeyNotFoundException("Game not found.");
        }

        public async Task<(bool Success, string Message)> AddGameToRepositoryAsync(int gameId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                var _game = _games[gameId];

                var userManager = scopedProvider.GetRequiredService<UserManager<User>>();

                var whitePlayer = _gameUserAssociations[gameId][1];
                var blackPlayer = _gameUserAssociations[gameId][2];
                var result = GetGameResult(gameId);
                User whiteUser = await userManager.FindByIdAsync(whitePlayer);
                User blackUser = await userManager.FindByIdAsync(blackPlayer);

                var mode = _game.gameMode;
                DateTime dateTime = DateTime.UtcNow;

                var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();


                GameMode gameMode = dataRepository.GameModeRepository.GetByIDAsync(1).Result;

                using var transaction = await dataRepository.BeginTransactionAsync();
                try
                {
                    var game = new Domain.Common.Game
                    {
                        WhitePlayer = whiteUser,
                        BlackPlayer = blackUser,
                        Date = dateTime,
                        Result = result,
                        GameMode = gameMode
                    };

                    await dataRepository.GameRepository.AddAsync(game);

                    await transaction.CommitAsync();
                    Console.WriteLine("Game added successfully.");
                    return (true, "Game added successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Failed to add game");
                    return (false, $"Failed to add game: {ex.Message}");
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
        public string GetGameResult(int gameId)
        {
            var game = _games[gameId];
            if (game.chessBoard.ifCheckmate(Color.Black)) return "1-0";
            if (game.chessBoard.ifCheckmate(Color.White)) return "0-1";
            if (game.chessBoard.isWhiteTimerOver == true) return "0-1";
            if (game.chessBoard.isBlackTimerOver == true) return "0-1";
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
