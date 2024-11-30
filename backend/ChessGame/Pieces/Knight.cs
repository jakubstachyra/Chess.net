using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    public class Knight : Piece
    {
        public Knight(Color color) : base(PieceType.Knight, color)
        {
        }

        public override bool IsMovePossible(Position start, Position end, ChessBoard chessBoard)
        {
            int deltaX = Math.Abs(start.x - end.x);
            int deltaY = Math.Abs(start.y - end.y);
            bool isLShape = ((deltaX==2 && deltaY==1) || (deltaX==1 && deltaY==2));

            if(!isLShape) 
            {
                return false;
            }     

            var pieceAtEnd = chessBoard.GetPieceAt(end);
            if (pieceAtEnd.pieceType != PieceType.None)
            {
                if (chessBoard.GetPieceAt(start).color == pieceAtEnd.color) return false;
            }
            return true;
        }
    }
}

