using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ChessGame.Pieces
{
    public class Bishop : Piece
    {
        public Bishop(Color color) : base(PieceType.Bishop, color)
        {
        }

        public override bool IsMovePossible(Position start, Position end, ChessBoard chessBoard)
        {

            int deltaX = start.x - end.x;
            int deltaY = start.y - end.y;
            if (deltaX == 0 && deltaY == 0) return false;
            bool isDiagonal = (Math.Abs(deltaX) == Math.Abs(deltaY));
            if (!isDiagonal)
            {
                return false;
            }
            int stepX = (end.x > start.x) ? 1 : -1;
            int stepY = (end.y > start.y) ? 1 : -1;

            int currentX = start.x+stepX;
            int currentY = start.y+stepY;

            while (currentX != end.x && currentY != end.y)
            {
               
                if (chessBoard.GetPieceAt(currentX, currentY).pieceType != PieceType.None) return false;
                currentX = currentX + stepX;
                currentY = currentY + stepY;
               
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

