using Chess.net.Services.Interfaces;
using ChessGame;
using ChessGame.AI;
using ChessGame.GameMechanics;
using System.Drawing;

namespace Chess.net.Services
{
    public class GameService: IGameService
    {
        private Game? _game;
        Algorithms algorithms;
        public void InitializeGame(int id)
        {
            if (_game == null)  // Only initialize if _game is null
            {
                _game = new Game(id);
                _game.StartGame(id);
                algorithms = new Algorithms(2);
            }
        }

        public List<Move> GetAllPlayerMoves() 
        {
            ChessGame.Color color = _game.player==0? ChessGame.Color.White:ChessGame.Color.Black;
            Console.WriteLine(_game.player);
            return _game.chessBoard.GetAllPlayerMoves(color);
        }



        public void MakeSentMove(string  move) 
        {
            Position start = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[0]}{move[1]}");
            Position end = ChessGame.Utils.Converter.ChessNotationToPosition($"{move[3]}{move[4]}");

            _game.ReceiveMove(start, end);
            _game.chessBoard.PrintBoard();
        }

        public Move CalculateBlackMove()
        {
           // algorithms.RunMinimaxAndPrintGraph(_game.chessBoard, ChessGame.Color.Black);
           var move = algorithms.Negamax(_game.chessBoard, algorithms.depth,ChessGame.Color.Black,int.MinValue,int.MaxValue).Item2;
            _game.ReceiveMove(move.from,move.to);
            _game.chessBoard.PrintBoard();
            return move;
        }

        public string WhoToMove()
        {
            return _game.player.ToString();
        }

        public string SendFen()
        {
            //_game.chessBoard/
            //return _game.chessBoard.GenerateFEN();
            return "";
        }
    }
}
