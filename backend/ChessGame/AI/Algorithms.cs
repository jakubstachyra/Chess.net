using ChessGame.GameMechanics;
using ChessGame.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.AI
{
    public class Algorithms
    {
        public int depth;

        public Algorithms(int depth)
        {
            this.depth = depth;
        }

        public (int, Move?) Minimax(ChessBoard chessBoard, int depth, bool isMaximizingPlayer, Color maximizingColor, int currentDepth = 0)
{
    if (depth == 0)
    {
        // When depth is 0, return the heuristic evaluation for the current board
        int eval = Heurestics.SimpleHeurestic(chessBoard.board, maximizingColor);
    //    Console.WriteLine($"{new string(' ', currentDepth * 4)}Depth {currentDepth}: Evaluation: {eval}");
        return (eval, null);
    }

    Color currentPlayerColor = isMaximizingPlayer ? maximizingColor : GetOppositeColor(maximizingColor);
    List<Move> allPlayerMoves = chessBoard.GetAllPlayerMoves(currentPlayerColor);
    Move bestMove = allPlayerMoves[0];

    if (isMaximizingPlayer)
    {
        int maxEval = int.MinValue;
        foreach (var move in allPlayerMoves)
        {
            ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
            boardCopy.MakeMoveWithoutChecking(move.from, move.to);

            // Print the current move and its depth
            //Console.WriteLine($"{new string(' ', currentDepth * 4)}Maximizing Move: {move} (Depth {currentDepth})");

            // Recurse for the minimizing player
            (int eval, Move movemade) = Minimax(boardCopy, depth - 1, false, maximizingColor, currentDepth + 1);

            // Print the evaluation after the recursion
            //Console.WriteLine($"{new string(' ', currentDepth * 4)}Evaluated Move: {move} with Eval: {eval} (Depth {currentDepth})");

            if (eval > maxEval)
            {
                maxEval = eval;
                bestMove = move;
            }
        }

        return (maxEval, bestMove);
    }
    else
    {
        int minEval = int.MaxValue;
        foreach (var move in allPlayerMoves)
        {
            ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
            boardCopy.MakeMoveWithoutChecking(move.from, move.to);

            // Print the current move and its depth
      //      Console.WriteLine($"{new string(' ', currentDepth * 4)}Minimizing Move: {move} (Depth {currentDepth})");

            // Recurse for the maximizing player
            (int eval, Move movemade) = Minimax(boardCopy, depth - 1, true, maximizingColor, currentDepth + 1);

            // Print the evaluation after the recursion
        //    Console.WriteLine($"{new string(' ', currentDepth * 4)}Evaluated Move: {move} with Eval: {eval} (Depth {currentDepth})");

            if (eval < minEval)
            {
                minEval = eval;
                bestMove = move;
            }
        }

        return (minEval, bestMove);
    }
}

        //negamax to do!!
        //alpha beta pruning to do!!!
        public (int, Move?) Negamax(ChessBoard chessBoard, int depth, Color color, int alfa, int beta)
        {
            if (depth == 0) return (Heurestics.SimpleHeurestic(chessBoard.board, color),null);
            List<Move> allPlayerMoves = chessBoard.GetAllPlayerMoves(color);
            Color oppositeColor = GetOppositeColor(color);
            int value = int.MinValue;
            if(allPlayerMoves.Count==0)  return (0, null); 
            Move BestMove = allPlayerMoves[0];
            foreach (var move in allPlayerMoves)
            {

                (int Evalue, Move EBestMove) = Negamax(chessBoard, depth - 1, oppositeColor,-beta ,-alfa);
                Evalue *= -1;
                if(Evalue>value) 
                {  BestMove = move;
                    value = Evalue;
                }
                alfa = Math.Max(alfa, value);
                if (alfa > beta) break;
                return (value, BestMove);
            }
            return (value, BestMove);
        }


        private Color GetOppositeColor(Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }

        public Move FindBestMove(ChessBoard chessBoard, Color maximizingColor)
        {
            int bestValue = int.MinValue;
            Move bestMove = null;

            List<Move> allPlayerMoves = chessBoard.GetAllPlayerMoves(maximizingColor);

            //foreach (var move in allPlayerMoves)
            //{
            //    ChessBoard boardCopy = chessBoard.CreateChessBoardCopy();
            //    boardCopy.MakeMove(move.from,move.to);
            //    //Console.WriteLine($"{move} {Heurestics.SimpleHeurestic(boardCopy.board, maximizingColor)};");



            //    if (moveValue > bestValue)
            //    {
            //        bestValue = moveValue;
            //        bestMove = move;
            //    }
            //}
            (int moveValue, bestMove) = Minimax(chessBoard, depth, false, maximizingColor);
            return bestMove;
        }

    }
}
