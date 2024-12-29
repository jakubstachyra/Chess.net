using ChessGame;
using ChessGame.GameMechanics;
using ChessGame.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.ChessLibraryTests
    {
        public class ChessLibraryCastleTests : ChessLibraryTestsBase
        {
            [Test]
            public void TestKingShortCastling()
            {
                ChessBoard chessBoard = new ChessBoard();
                string fen = "R3K2R/pppppppp/8/8/8/8/PPPPPPPP/r3k2r w KQkq - 0 1";
                chessBoard.LoadFEN(fen);

                Piece king = chessBoard.board[4, 0];  
                Piece rook = chessBoard.board[7, 0];
                Position kingStart = new Position(4, 0);
                Position rookStart = new Position(7, 0);

                rook.isMoved = false;
                king.isMoved = false;

                Move shortCastlingMove = new Move(kingStart, new Position(6, 0)); 

                List<Move> possibleKingMoves = chessBoard.GetAllPieceMoves(king);

                Assert.IsTrue(possibleKingMoves.Contains(shortCastlingMove), "Brak możliwości roszady krótkiej dla króla.");
            }

            [Test]
            public void TestKingLongCastling()
            {
                ChessBoard chessBoard = new ChessBoard();
            string fen = "R3K2R/pppppppp/8/8/8/8/PPPPPPPP/r3k2r w KQkq - 0 1";
            chessBoard.LoadFEN(fen);

                Piece king = chessBoard.board[4, 0]; 
                Piece rook = chessBoard.board[0, 0]; 
                Position kingStart = new Position(4, 0);
                Position rookStart = new Position(0, 0);

                Move longCastlingMove = new Move(kingStart, new Position(2, 0));

                List<Move> possibleKingMoves = chessBoard.GetAllPieceMoves(king);

                Assert.IsTrue(possibleKingMoves.Contains(longCastlingMove), "Brak możliwości roszady długiej dla króla.");
            }

            [Test]
            public void TestKingInCheckCannotCastle()
            {
                ChessBoard chessBoard = new ChessBoard();
                string fen = "RNBQKBNR/8/8/8/8/8/8/rnbkq2r w KQkq - 0 1";

            chessBoard.LoadFEN(fen);

                Piece king = chessBoard.board[4, 0]; 
                Piece rook = chessBoard.board[7, 0];  

                
                List<Move> possibleKingMoves = chessBoard.GetAllPieceMoves(king);

                
                Assert.IsFalse(possibleKingMoves.Any(m => m.to.x == 6 && m.to.y == 0), "Król może wykonać roszadę, ale nie powinien, ponieważ jest w szachu.");
            }

            [Test]
            public void TestCastlingBlockedByPiece()
            {
                
                ChessBoard chessBoard = new ChessBoard();
                string fen = "RNBQKBNR/8/8/8/8/8/8 w KQkq - 0 1"; 

            chessBoard.LoadFEN(fen);

                Piece king = chessBoard.board[4, 0]; 
                Piece rook = chessBoard.board[7, 0];  

                
                List<Move> possibleKingMoves = chessBoard.GetAllPieceMoves(king);

                Assert.IsFalse(possibleKingMoves.Any(m => m.to.x == 6 && m.to.y == 0), "Król może wykonać roszadę, ale nie powinien, ponieważ droga jest zablokowana.");
            }

            [Test]
            public void TestCastlingImpossibleIfPiecesMoved()
            {
                
                ChessBoard chessBoard = new ChessBoard();
            string fen = "R3K2R/pppppppp/8/8/8/8/PPPPPPPP/r3k2r w KQkq - 0 1";

            chessBoard.LoadFEN(fen);

                
                Piece rook = chessBoard.board[7, 0]; 
                rook.isMoved = true; 

                Piece king = chessBoard.board[4, 0];  

                
                List<Move> possibleKingMoves = chessBoard.GetAllPieceMoves(king);

                Assert.IsFalse(possibleKingMoves.Any(m => m.to.x == 6 && m.to.y == 0), "Król powinien nie móc wykonać roszady, ponieważ wieża się poruszyła.");
            }
        }
    }



