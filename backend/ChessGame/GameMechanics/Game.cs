using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.GameMechanics
{
    public class Game
    {
        int id;
        public ChessBoard chessBoard { get; private set; }
        string result;
        static Color [] players = [Color.White, Color.Black];
        List<Move> moves = new List<Move>();
        public string gamestatus = "N";
        public string gameMode;
        public List<MoveHistoryEntry> MoveHistory { get; set; } = new List<MoveHistoryEntry>();
        public int MovesSoFar { get; set; } = 0;
        public int player { get; private set; }
        public Game(int id) 
        {
            this.id = id;
            player = 0;
            chessBoard = new ChessBoard();
            result = "";
            gameMode = "";
        }

        public void StartGame(int _id)
        {   
            //int player = 0; //0-white 1-black
            //while (true)
            //{
            //    //main game loop
            //    //input move
            //    //check if is legal
            //    //check if check after move
            //    //check if checkmate
            //    //check if promotion
            //    //check if game end

            //    Console.Clear();



            //    chessBoard.PrintBoard();
               

            //}
        }

        public void ReceiveMove(string start, string end)
        {

            Position startP = Utils.Converter.ChessNotationToPosition(start);
            Position endP = Utils.Converter.ChessNotationToPosition(end);

            if (chessBoard.board[startP.x, startP.y].color == players[player] && chessBoard.board[startP.x, startP.y].IsMovePossible(startP, endP, chessBoard))
            {
                chessBoard.MakeMove(startP, endP);
                moves.Add(new Move(startP, endP));
                player++;
                player %= 2;
            }

            Color color = player==0 ? Color.White : Color.Black;
            if(chessBoard.ifCheckmate(color))
            {
                gamestatus = color.ToString();
            }
        }

        public void ReceiveMove(Position startP, Position endP)
        {


            if (chessBoard.board[startP.x, startP.y].color == players[player] && chessBoard.board[startP.x, startP.y].IsMovePossible(startP, endP, chessBoard))
            {
                chessBoard.MakeMove(startP, endP);
                moves.Add(new Move(startP, endP));
                player++;
                player %= 2;
            }
            Color color = player == 0 ? Color.White : Color.Black;
            if (chessBoard.ifCheckmate(color))
            {
                gamestatus = color.ToString();
            }
        }
        public void PrintBoard()
        {
            this.chessBoard.PrintBoard();
        }
        
    }
    public class MoveHistoryEntry
    {
        public int MoveNumber { get; set; }
        public string Fen { get; set; }
        public string Move { get; set; }
        public int? WhiteRemainingTimeMs { get; set; }
        public int? BlackRemainingTimeMs { get; set; }
    }

}
