using ChessGame;
using ChessGame.AI;
using ChessGame.GameMechanics;
using System.Drawing;
using System.Collections.Concurrent;
using ChessGame.Pieces;
using Color = ChessGame.Color;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DataContext;
using System.Security.Claims;
using Logic.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;

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

        public GameService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public int InitializeGameWithComputer(string userIdPlayer1= "guest")
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
                { 2, null }
            };

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

        public ChessGame.GameMechanics.Move CalculateComputerMove(int gameId)
        {
            Console.WriteLine(gameId);

            if (_games.TryGetValue(gameId, out var game) && _gameAlgorithms.TryGetValue(gameId, out var algorithms))
            {
                if (!game.chessBoard.ifCheckmate(Color.Black))
                {
                    var move = algorithms.Negamax(game.chessBoard, algorithms.depth, ChessGame.Color.Black, int.MinValue, int.MaxValue).Item2;
                    game.ReceiveMove(move.from, move.to);
                    return move;
                }
                else
                {
                    return new ChessGame.GameMechanics.Move(new Position(0, 0), new Position(0, 0));
                }
            }

            throw new KeyNotFoundException("Game or algorithms not found.");
        }

        public async Task<bool> GetGameState(int gameId)
        {
                
            if (_games.TryGetValue(gameId, out var game))
            {
                var color1 = game.player == 0 ? Color.White : Color.Black;
                var color2 = game.player == 0 ? Color.Black : Color.White;
;
                if (game.chessBoard.ifCheckmate(color1))
                {
                   await AddGameToRepositoryAsync(gameId);
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

                var userManager = scopedProvider.GetRequiredService<UserManager<User>>();
                var user = _gameUserAssociations[gameId][1];
                User white = await userManager.FindByIdAsync(user);
                
                DateTime dateTime = DateTime.UtcNow;

                var dataRepository = scopedProvider.GetRequiredService<IDataRepository>();


                GameMode gameMode = dataRepository.GameModeRepository.GetByIDAsync(1).Result;
                Console.WriteLine(gameMode.ToString());
                using var transaction = await dataRepository.BeginTransactionAsync();
                try
                {
                    var game = new Domain.Common.Game
                    {
                        WhitePlayer = white,
                        BlackPlayer = white,
                        Date = dateTime,
                        Result = "1-0",
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
