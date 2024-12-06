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
            // Znajdź pierwszy dostępny indeks
            int newGameId = FindFirstAvailableGameId();

            // Dodaj grę z nowym identyfikatorem
            _games.GetOrAdd(newGameId, _ =>
            {
                var game = new Game(newGameId);
                game.StartGame(newGameId);
                _gameAlgorithms[newGameId] = new Algorithms(2); // Initialize AI algorithms
                return game;
            });
            Console.WriteLine($"count: {_games.Count()}");
            Console.WriteLine($"id: {newGameId}");
            return newGameId;
        }



        private int FindFirstAvailableGameId()
        {
            int gameId = 1; // Zaczynamy od ID = 1

            // Szukaj pierwszego ID, które nie istnieje w słowniku
            while (_games.ContainsKey(gameId))
            {
                gameId++;
            }

            return gameId;
        }


        public List<Move> GetAllPlayerMoves(int gameId)
        {
            Console.WriteLine($"id api: {gameId}");
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
                    Position end = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[3]}{move[4]}");
                    game.ReceiveMove(start, end);
                    game.chessBoard.PrintBoard();
                }
                else
                {
                    throw new KeyNotFoundException("Game not found.");
                }
            }

            public Move CalculateBlackMove(int gameId)
            {
                if (_games.TryGetValue(gameId, out var game) && _gameAlgorithms.TryGetValue(gameId, out var algorithms))
                {
                    var move = algorithms.Negamax(game.chessBoard, algorithms.depth, ChessGame.Color.Black, int.MinValue, int.MaxValue).Item2;
                    game.ReceiveMove(move.from, move.to);
                    game.chessBoard.PrintBoard();
                    return move;
                }

                throw new KeyNotFoundException("Game or algorithms not found.");
            }

            public string WhoToMove(int gameId)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    return game.player.ToString();
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


