using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    public abstract class Piece : IMove
    {
        public PieceType pieceType;
        public Color color;
        public bool isMoved = false;
        public Position position;
        public Piece(PieceType pieceType, Color color)
        {
            this.pieceType = pieceType;
            this.color = color;
        }

        public void setPosition(Position position)
        {
            this.position = position;
        }

        public void setPosition(int x, int y)
        {
            this.position = new Position(x,y);
        }
        public Piece Clone()
        {
            Piece clone = PieceFactory.CreatePiece(this.pieceType, this.color);
            clone.setPosition(this.position);
            return clone;
        }

        public abstract bool IsMovePossible(Position start, Position end, ChessBoard chessBoard);
    }
}
