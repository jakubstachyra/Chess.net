using System;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using ChessGame.AI;
using ChessGame.GameMechanics;
using ChessGame.Pieces;

namespace ChessGame
{
    public interface IMove
    {
        bool IsMovePossible(Position start, Position end, ChessBoard chessBoard);
    }
    public class ChessGame
    {


        public static void Main(string[] args)
        {


            Game game = new Game(1);
            Algorithms algorithms = new Algorithms(2);
            while (true)
            {

                game.PrintBoard();
                //Console.WriteLine(game.player);

                //Console.WriteLine(game.player);
                if (game.player == 0)
                {
                    string start = Console.ReadLine();
                    string end = Console.ReadLine();
                    Position startP = Utils.Converter.ChessNotationToPosition(start);
                    Position endP = Utils.Converter.ChessNotationToPosition(end);
                    game.ReceiveMove(start, end);
                }

                Console.Write("Pozycje:");
                foreach (var a in game.chessBoard.GetAllPlayerMoves(Color.Black))
                    Console.WriteLine(a);
                //game.PrintBoard();
                //Console.WriteLine(game.player);

                if (game.player == 1)
                {
                    Move move = algorithms.Minimax(game.chessBoard, 2, true, Color.Black).Item2;
                    //Console.WriteLine(move);
                    game.ReceiveMove(move.from, move.to);

                }
                //Console.Clear();
                //game.PrintBoard();

                Console.WriteLine(game.player);

            }
            ///ChessBoard board = new ChessBoard();
            //board.board[1, 0] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            //board.board[2, 0] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            //board.board[3, 0] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            //board.board[4, 4] = PieceFactory.CreatePiece(PieceType.Queen, Color.White);
            //board.board[4, 4].setPosition(4,4);
            //Console.Write("Pozycje:");
            //foreach(var a in board.GetAllPlayerMoves(Color.White))
            //Console.WriteLine($"{a.from} -> {a.to}");
            while (true)
            {

                //Console.WriteLine(board.board[5, 1]);
                //Console.WriteLine(board.board[1,5].color);

                ////board.board[1, 0] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                //board.PrintBoard();
                //List<Move> moves = board.GetAllPlayerMoves(Color.White);
                //Console.Clear();
                //Console.WriteLine(Utils.Converter.ChessNotationToPosition("A4"));
                //foreach (Move move in moves)
                //{
                //    //Console.WriteLine($"{move.from} {move.to}");
                //    //Console.WriteLine($"{move.from.x} {move.from.y} =>  {move.to.x} {move.to.y}");
                //}
                //Console.WriteLine(board.board[1, 5]);
                //Console.WriteLine(board.board[0, 4]);



                //board.PrintBoard();
                //string start = Console.ReadLine();
                //string end = Console.ReadLine();
                //Position startP = Utils.Converter.ChessNotationToPosition(start);
                //Position endP = Utils.Converter.ChessNotationToPosition(end);
                ////Position startP = new Position(int.Parse(start[0].ToString()), int.Parse(start[1].ToString()));
                ////Position endP = new Position(int.Parse(end[0].ToString()), int.Parse(end[1].ToString()));
                //if (board.board[startP.x, startP.y].IsMovePossible(startP, endP, board)) board.MakeMove(startP, endP);

            }
        }
    }
}
