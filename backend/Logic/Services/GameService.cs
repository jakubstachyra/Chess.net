using Chess.net.Services.Interfaces;
using ChessGame;
using ChessGame.AI;
using ChessGame.GameMechanics;
using System.Drawing;
using System.Collections.Concurrent;

namespace Chess.net.Services
{
    public class GameService: IGameService
    {
        private readonly ConcurrentDictionary<int, Game> _games = new();
        private readonly ConcurrentDictionary<int, Algorithms> _gameAlgorithms = new();

        public int InitializeGame()
        {
            int newGameId = FindFirstAvailableGameId();

            _games.GetOrAdd(newGameId, _ =>
            {
                var game = new Game(newGameId);
                game.StartGame(newGameId);
                _gameAlgorithms[newGameId] = new Algorithms(2);
                return game;
            });
            Console.WriteLine($"id: {newGameId}");
            return newGameId;
        }



        private int FindFirstAvailableGameId()
        {
            int gameId = 1;
            while (_games.ContainsKey(gameId))
            {   
                gameId++;
            }

            return gameId;
        }


        public List<Move> GetAllPlayerMoves(int gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                var color = game.player == 0 ? ChessGame.Color.White : ChessGame.Color.Black;
                var a =game.chessBoard.GetAllPlayerMoves(color);
                foreach (var b in a)
                    Console.WriteLine(b);
                return game.chessBoard.GetAllPlayerMoves(color);
            }

            throw new KeyNotFoundException("Game not found.");
        }

        public void MakeSentMove(int gameId, string move)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    Position start = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[0]}{move[1]}");
                    Position end = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[3]}{move[4]}");
                Console.Write("move: ");
                Console.WriteLine(move);
                    game.ReceiveMove(start, end);
                game.PrintBoard();
                }
                else
                {
                    throw new KeyNotFoundException("Game not found.");
                }
            }

            public Move CalculateComputerMove(int gameId)
            {
            Console.WriteLine(gameId);
                if (_games.TryGetValue(gameId, out var game) && _gameAlgorithms.TryGetValue(gameId, out var algorithms))
                {
                    var move = algorithms.Negamax(game.chessBoard, algorithms.depth, ChessGame.Color.Black, int.MinValue, int.MaxValue).Item2;
                    game.ReceiveMove(move.from, move.to);
                    return move;
                }

                throw new KeyNotFoundException("Game or algorithms not found.");
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
        }
    }


