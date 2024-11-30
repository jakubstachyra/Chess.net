using ChessGame.Pieces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.AI
{
    public class Heurestics
    {
        public static Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>()
        {
            {PieceType.None,0},
            {PieceType.Pawn, 1},
            {PieceType.Knight,3 },
            {PieceType.Bishop,3 },
            {PieceType.Rook,5 },
            {PieceType.Queen,9 },
            {PieceType.King,1000}

        };
        public static int SimpleHeurestic(Piece[,] board, Color maximazingColor)
        {
            
            int value = 0; 
            for(int i=0;i<board.GetLength(1);i++)
                for(int j=0;j<board.GetLength(0);j++)
                {
                    Piece piece = board[i, j];
                    int pieceValue = PieceValues[piece.pieceType];
                    if (piece.color == Color.White) value = value + pieceValue;
                    if(piece.color == Color.Black) value = value - pieceValue;
                }
            if (maximazingColor == Color.Black) return -value;
            return value;
        }
    }
}
