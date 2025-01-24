using ChessGame.Pieces;
using System;
using System.Collections.Generic;
using System.Data;
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
        static Color[] players = [Color.White, Color.Black];
        List<Move> moves = new List<Move>();
        public string gamestatus = "N";
        public string gameMode;
        public List<MoveHistoryEntry> MoveHistory { get; set; } = new List<MoveHistoryEntry>();
        public int MovesSoFar { get; set; } = 0;
        public List<int> moveRemaingTimes = new List<int>();
        public int fiftyMoveRuleCounter = 0;
        Dictionary<string, int> positionHistory = new Dictionary<string, int>();
        string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq";
        public string[] modes = ["960", "brain-hand", "The king is dead, long live the king!"];
        public int player { get; private set; }

        public bool acceptedDrawOffer = false;
        public bool whiteResigned = false;
        public bool blackResigned = false;

        public Piece whiteNewKing;
        public Piece blackNewKing;
        public Game(int id, string mode="", string randomFen="")
        {
            this.id = id;
            gameMode = mode;
            player = 0;
            chessBoard = new ChessBoard(mode);
            if (gameMode == modes[0])
            {
                Console.WriteLine("fisher");
                Console.WriteLine(randomFen);
                chessBoard.LoadFEN(randomFen);
            }

            if (gameMode == modes[2])
            {
                drawNewKing();
            }
            result = "";

            positionHistory[startFen] = 1;
        }

        public void drawNewKing()
        {
            Random random = new Random();
            int number;

            do
            {
                number = random.Next(0, 8);
            } while (number == 3);
            whiteNewKing = chessBoard.GetPieceAt(new Position(number, 0));

            do
            {
                number = random.Next(0, 8);
            } while (number == 3);
            blackNewKing = chessBoard.GetPieceAt(new Position(number, 7));
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

            Color color = player == 0 ? Color.White : Color.Black;
            if (chessBoard.ifCheckmate(color))
            {
                gamestatus = color.ToString();
            }
        }

        public void ReceiveMove(Position startP, Position endP)
        {
            if (gameMode == modes[2])
            {
                Console.WriteLine($"{whiteNewKing.position.x} {whiteNewKing.position.y} {whiteNewKing.pieceType}");

                Console.WriteLine($"{blackNewKing.position.x} {blackNewKing.position.y} {blackNewKing.pieceType}");
            }
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

        public (bool, string reason) isDraw()
        { 
                TrackPosition();

                if (IsStalemate()) return (true, "Stalemate");

                if (IsInsufficientMaterial()) return (true, "Insufficient material");

                if (IsThreefoldRepetition()) return (true, "Threefold repetition");

                if (IsFiftyMoveRuleDraw()) return (true, "Fifty-move rule");

                return (false, "");
            }



        



        bool IsFiftyMoveRuleDraw()
        {
            return fiftyMoveRuleCounter >= 100; //50 for each player
        }
        bool IsThreefoldRepetition()
        {
            string positionKey = chessBoard.GenerateFEN();
            positionKey = positionKey.Substring(0, positionKey.Length - 6);

            if (positionHistory.ContainsKey(positionKey) && positionHistory[positionKey] >= 3 && MoveHistory.Count != 0)
            {
                return true;
            }
            return false;

        }
        public bool IsInsufficientMaterial()
        {
            var pieces = GetAllPieces();
            if (pieces.Count == 2)
            {
                return true;
            }
            if (pieces.Count == 3)
            {
                var piece = pieces.FirstOrDefault(p => p.pieceType != PieceType.King);
                if (piece != null && (piece.pieceType == PieceType.Knight || piece.pieceType == PieceType.Bishop))
                {
                    return true;
                }
            }

            if (pieces.Count == 4)
            {
                var whiteBishop = pieces.FirstOrDefault(p => p.pieceType == PieceType.Bishop && p.color == Color.White);

                var blackBishop = pieces.FirstOrDefault(p => p.pieceType == PieceType.Bishop && p.color == Color.Black);
                if (whiteBishop != null && blackBishop != null)
                {
                    var whiteBishopPosition = whiteBishop.position;
                    var blackBishopPosition = blackBishop.position;
                    if ((whiteBishopPosition.x + whiteBishop.position.y) % 2 == (blackBishopPosition.x + blackBishop.position.y) % 2) return true;

                }

            }
            return false;
        }

        void TrackPosition()
        {
            string rawFen = chessBoard.GenerateFEN();
            Console.WriteLine("Raw FEN: " + rawFen);
            string positionKey = rawFen.Substring(0, rawFen.Length - 6).Trim();

            if (positionHistory.ContainsKey(positionKey))
            {
                positionHistory[positionKey]++;
            }
            else
            {
                positionHistory[positionKey] = 1;
            }

        }


        public List<Piece> GetAllPieces()
        {
            List<Piece> pieces = new List<Piece>();
            for (int i = 0; i < chessBoard.row; i++)
                for (int j = 0; j < chessBoard.row; j++)
                    if (chessBoard.board[i, j].pieceType != PieceType.None) pieces.Add(chessBoard.board[i, j]);
            return pieces;
        }
        public bool IsStalemate()
        {
            var color = player == 0 ? Color.White : Color.Black;
            if (chessBoard.GetAllPlayerMoves(color).Count == 0 && chessBoard.ifCheckmate(color) == false) return true;
            return false;
        }

     public bool The_king_is_dead_long_live_the_king_Endgame(Color color)
        {

            if (chessBoard.GetPieceAt( whiteNewKing.position).color !=  Color.White && color==Color.White) return true;
            if (chessBoard.GetPieceAt(blackNewKing.position).color == Color.Black && color==Color.Black) return true;
            return false;

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
