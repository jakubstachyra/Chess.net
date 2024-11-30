using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    public static class PieceFactory
    {
        public static Piece CreatePiece(PieceType pieceType, Color color)
        {
            switch (pieceType)
            {
                case PieceType.Pawn:
                    return new Pawn(color);
                case PieceType.Rook:
                    return new Rook(color);
                case PieceType.Knight:
                    return new Knight(color);
                case PieceType.Bishop:
                    return new Bishop(color);
                case PieceType.King:
                    return new King(color);
                case PieceType.Queen:
                    return new Queen(color);
                case PieceType.None:
                    return new None(color);
                default:
                    throw new ArgumentException("Invalid piece type");
            }
        }
    }

    public class None : Piece
    {
        public None(Color color) : base(PieceType.None, color)
        {
        }

        public override bool IsMovePossible(Position start, Position end, ChessBoard chessBoard)
        {
            return true;
        }
    }
}
