using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    public class Pawn : Piece
    {
        public Pawn(Color color) : base(PieceType.Pawn, color)
        {
        }
        
        public override bool IsMovePossible(Position start, Position end, ChessBoard chessBoard)
        {

            int deltaX = start.x - end.x;
            int deltaY = start.y - end.y;

            int direction = (this.color == Color.White) ? -1 : 1;

            var pieceAtEnd = chessBoard.GetPieceAt(end);
            if(deltaX==0)
            {
                if(deltaY==direction)
                {

                    
                  
                    if (pieceAtEnd.pieceType == PieceType.None)
                    {
                       return true;
                    }
                }
                if(deltaY==2*direction && this.isMoved==false)
                {
                    if(pieceAtEnd.pieceType==PieceType.None && chessBoard.GetPieceAt(start.x,start.y-direction).pieceType==PieceType.None)
                    {
                        return true;
                    }
                }
                return false;
            }
            if(Math.Abs(deltaX)==1)
            {
                if (deltaY == direction && pieceAtEnd.color != this.color && pieceAtEnd.pieceType != PieceType.None)
                    return true;
            }
            return false;
            if (Math.Abs(deltaX) > 1) return false;
            if(Math.Abs(deltaX) ==1)
            {
               if(Math.Abs(deltaY)==1 && pieceAtEnd.pieceType != PieceType.None && pieceAtEnd.color != this.color)return true;
                return false;
            }

            if(Math.Abs(deltaY)>1)
            {
                if (Math.Abs(deltaY) == 2 && this.isMoved == false) return true;
            }
        }
    }
}
