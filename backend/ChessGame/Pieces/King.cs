using ChessGame.GameMechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    public class King : Piece
    {
        public King(Color color) : base(PieceType.King, color)
        {
        }

        public override bool IsMovePossible(Position start, Position end, ChessBoard chessBoard)
        {
            int deltaX = start.x - end.x;
            int deltaY = start.y - end.y;

            if (Math.Abs(deltaX) == 2 && deltaY == 0)
            {
                return CanCastle(start, end, chessBoard, deltaX);
            }
            if (deltaX == 0 && deltaY == 0) return false;
            

            if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1) return false;

            var pieceAtEnd = chessBoard.GetPieceAt(end);
            if (pieceAtEnd.pieceType != PieceType.None)
            {
                if (chessBoard.GetPieceAt(start).color == pieceAtEnd.color) return false;
            }
            return true;
        }

        public bool CanCastle(Position start, Position end, ChessBoard chessBoard, int deltaX)
        {

            Position[] whiteRooks = [new Position(0, 0), new Position(chessBoard.row - 1, 0)];
            Position[] blackRooks = [new Position(0,7), new Position(chessBoard.row - 1, 7)];
            Piece pieceStart = chessBoard.GetPieceAt(start);
            if (pieceStart.isMoved) return false;
            Color color = pieceStart.color;
            if (chessBoard.IsKingInCheck(color)) return false;
            if (deltaX == -2)
            {
                if (color == Color.White)
                {

                    Position rookPosition = whiteRooks[1];
                    Piece rook = chessBoard.GetPieceAt(rookPosition);
                    if (rook.pieceType != PieceType.Rook || rook.color != Color.White || rook.isMoved) return false;
                    Position[] positionsBetween = { new Position(start.x + 1, start.y), new Position(start.x + 2, start.y)};
                    foreach (Position position in positionsBetween)
                    {

                        if (chessBoard.GetPieceAt(position).pieceType != PieceType.None) return false;
                       

                        ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
                        boardCopy.board[position.x, position.y] = boardCopy.board[start.x, start.y];
                        boardCopy.board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

                        if ((boardCopy.IsKingInCheck(color)))
                        {
                            return false;
                        }

                    }

                }

                if (color == Color.Black)
                {
                    Position rookPosition = blackRooks[1];
                    Piece rook = chessBoard.GetPieceAt(rookPosition);
                    if (rook.pieceType != PieceType.Rook || rook.color != Color.Black || rook.isMoved) return false;
                    Position[] positionsBetween = { new Position(start.x + 1, start.y), new Position(start.x + 2, start.y) };
                    foreach (Position position in positionsBetween)
                    {
                        if (chessBoard.GetPieceAt(position).pieceType != PieceType.None) return false;
                        ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
                        boardCopy.board[position.x, position.y] = boardCopy.board[start.x, start.y];
                        boardCopy.board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                        if ((boardCopy.IsKingInCheck(color)))
                        {
                            return false;
                        }

                    }
                }
            }
            if (deltaX == 2)
            {
                
                if (color == Color.White)
                {
                    Position rookPosition = whiteRooks[0];
                    Piece rook = chessBoard.GetPieceAt(rookPosition);

                    if (rook.pieceType != PieceType.Rook || rook.color != Color.White || rook.isMoved) return false;

                    Position[] positionsBetween = { new Position(start.x - 1, start.y), new Position(start.x - 2, start.y), new Position(start.x - 3, start.y) };
                    foreach (Position position in positionsBetween)
                    {
                        if (chessBoard.GetPieceAt(position).pieceType != PieceType.None) return false;
                        ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
                        boardCopy.board[position.x, position.y] = boardCopy.board[start.x, start.y];
                        boardCopy.board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                        if ((boardCopy.IsKingInCheck(color)))
                        {

                            return false;
                        }

                    }

                }

                if (color == Color.Black)
                {
                    Position rookPosition = blackRooks[0];
                    Piece rook = chessBoard.GetPieceAt(rookPosition);

                    if (rook.pieceType != PieceType.Rook || rook.color != Color.Black || rook.isMoved) return false;
                    Position[] positionsBetween = { new Position(start.x - 1, start.y), new Position(start.x - 2, start.y), new Position(start.x - 3, start.y) };
                    foreach (Position position in positionsBetween)
                    {
                        if (chessBoard.GetPieceAt(position).pieceType != PieceType.None) return false;
                        ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
                        boardCopy.board[position.x, position.y] = boardCopy.board[start.x, start.y];
                        boardCopy.board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                        if ((boardCopy.IsKingInCheck(color)))
                        {
                            return false;
                        }

                    }
                }
            }
            return true;

        }
        //public bool CanCastle(Position start, Position end, ChessBoard chessBoard, int deltaX)
        //{

        //    Position[] whiteRooks = [new Position(0, 0), new Position(0, chessBoard.column - 1)];
        //    Position[] blackRooks = [new Position(chessBoard.row - 1, 0), new Position(chessBoard.row - 1, chessBoard.column - 1)];
        //    Piece pieceStart = chessBoard.GetPieceAt(start);
        //    if (pieceStart.isMoved) return false;
        //    Color color = pieceStart.color;
        //    if (chessBoard.ifCheck(color)) return false;

        //    if (deltaX == -2)
        //    {
        //        if (color == Color.White)
        //        {
        //            if(start.x!=)
        //        }
        //    }
        //}
        //public bool CanCastle(Position start, Position end, ChessBoard chessBoard, int deltaX)
        //{
        //    Piece pieceStart = chessBoard.GetPieceAt(start);
        //    if (pieceStart.isMoved) return false;
        //    Color color = pieceStart.color;
        //    if (chessBoard.ifCheck(color)) return false;
        //    if (deltaX == -2) //prawa roszada
        //    {
        //        if (color == Color.White)
        //        {

        //        }
        //    }
        //}
    }
}

