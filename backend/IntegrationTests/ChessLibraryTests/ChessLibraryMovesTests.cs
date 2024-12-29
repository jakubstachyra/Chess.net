using ChessGame;
using ChessGame.GameMechanics;
using ChessGame.Pieces;
using Domain.AuthModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AccountController;

namespace IntegrationTests.ChessLibraryTests
{
    public class ChessLibraryMovesTests : ChessLibraryTestsBase
    {



        [Test]
        public void White_Has_20_StartingMoves_Black_Has_20()
        {
            var colorWhite = ChessGame.Color.White;
            var colorBlack = ChessGame.Color.Black;

            var movesWhite = game.chessBoard.GetAllPlayerMoves(colorWhite);
            var movesBlack = game.chessBoard.GetAllPlayerMoves(colorBlack);

            Assert.AreEqual(movesWhite.Count, 20);
            Assert.AreEqual(movesBlack.Count, 20);

        }

        [Test]
        public void TestRookMoves()
        {
            
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3R4/8/8/8/8 w - - 0 1";
            chessBoard.LoadFEN(fen);

            Piece rook = chessBoard.board[3, 3];

           
            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(rook);
            Position from = new Position(3, 3);

           
            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(3, 0)),
        new Move(from, new Position(3, 2)),
        new Move(from, new Position(3, 4)),
        new Move(from, new Position(3, 5)),
        new Move(from, new Position(3, 6)),
        new Move(from, new Position(3, 7)),
        new Move(from, new Position(0, 3)),
        new Move(from, new Position(1, 3)),
        new Move(from, new Position(2, 3)),
        new Move(from, new Position(4, 3)),
        new Move(from, new Position(5, 3)),
        new Move(from, new Position(6, 3)),
        new Move(from, new Position(3, 1)),
        new Move(from, new Position(7, 3))
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);

        }


        [Test]
        public void TestKingMoves()
        {
           
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3K4/8/8/8/8 w - - 0 1";
            chessBoard.LoadFEN(fen);

            Piece king = chessBoard.board[3, 3];

            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(king);
            Position from = new Position(3, 3);

            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(2, 2)),
        new Move(from, new Position(2, 3)),
        new Move(from, new Position(2, 4)),
        new Move(from, new Position(3, 2)),
        new Move(from, new Position(3, 4)),
        new Move(from, new Position(4, 2)),
        new Move(from, new Position(4, 3)),
        new Move(from, new Position(4, 4))
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);
        }

        [Test]
        public void TestQueenMoves()
        {
        
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3Q4/8/8/8/8 w - - 0 1"; 
            chessBoard.LoadFEN(fen);

            Piece queen = chessBoard.board[3, 3];

            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(queen);
            Position from = new Position(3, 3);

            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(3, 0)), new Move(from, new Position(3, 1)),
        new Move(from, new Position(3, 2)), new Move(from, new Position(3, 4)),
        new Move(from, new Position(3, 5)), new Move(from, new Position(3, 6)),
        new Move(from, new Position(3, 7)), new Move(from, new Position(0, 3)),
        new Move(from, new Position(1, 3)), new Move(from, new Position(2, 3)),
        new Move(from, new Position(4, 3)), new Move(from, new Position(5, 3)),
        new Move(from, new Position(6, 3)), new Move(from, new Position(7, 3)),
        
        new Move(from, new Position(0, 0)), new Move(from, new Position(1, 1)),
        new Move(from, new Position(2, 2)), new Move(from, new Position(4, 4)),
        new Move(from, new Position(5, 5)), new Move(from, new Position(6, 6)),
        new Move(from, new Position(7, 7)), new Move(from, new Position(0, 6)),
        new Move(from, new Position(1, 5)), new Move(from, new Position(2, 4)),
        new Move(from, new Position(4, 2)), new Move(from, new Position(5, 1)),
        new Move(from, new Position(6, 0))
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);
        }

        [Test]
        public void TestBishopMoves()
        {
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3B4/8/8/8/8 w - - 0 1";
            chessBoard.LoadFEN(fen);

            Piece bishop = chessBoard.board[3, 3];

            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(bishop);
            Position from = new Position(3, 3);

            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(0, 0)), new Move(from, new Position(1, 1)),
        new Move(from, new Position(2, 2)), new Move(from, new Position(4, 4)),
        new Move(from, new Position(5, 5)), new Move(from, new Position(6, 6)),
        new Move(from, new Position(7, 7)), new Move(from, new Position(0, 6)),
        new Move(from, new Position(1, 5)), new Move(from, new Position(2, 4)),
        new Move(from, new Position(4, 2)), new Move(from, new Position(5, 1)),
        new Move(from, new Position(6, 0))
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);
        }

        [Test]
        public void TestKnightMoves()
        {
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3N4/8/8/8/8 w - - 0 1";
            chessBoard.LoadFEN(fen);

            Piece knight = chessBoard.board[3, 3];

            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(knight);
            Position from = new Position(3, 3);

            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(1, 2)), new Move(from, new Position(1, 4)),
        new Move(from, new Position(2, 1)), new Move(from, new Position(2, 5)),
        new Move(from, new Position(4, 1)), new Move(from, new Position(4, 5)),
        new Move(from, new Position(5, 2)), new Move(from, new Position(5, 4))
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);
        }

        [Test]
        public void TestPawnMoves()
        {
            ChessBoard chessBoard = new ChessBoard();
            string fen = "8/8/8/3P4/8/8/8/8 w - - 0 1";
            chessBoard.LoadFEN(fen);

            Piece pawn = chessBoard.board[3, 3];
            pawn.isMoved = true;
            List<Move> possibleMoves = chessBoard.GetAllPieceMoves(pawn);
            Position from = new Position(3, 3);

            List<Move> expectedMoves = new List<Move>
    {
        new Move(from, new Position(3, 4)) 
    };

            foreach (var move in expectedMoves)
            {
                Assert.IsTrue(possibleMoves.Contains(move), $"Brakuje ruchu: From: ({move.from.x}, {move.from.y}) To: ({move.to.x}, {move.to.y})");
            }
            Assert.AreEqual(expectedMoves.Count, possibleMoves.Count);
        }

    }
}