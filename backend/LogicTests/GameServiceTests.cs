using Moq;
using NUnit.Framework;
using ChessGame;
using Chess.net.Services;
using ChessGame.GameMechanics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ChessGame.AI;
using System.Reflection;

namespace Chess.net.Tests
{
    public class GameServiceTests
    {
        private GameService _gameService;
        private Mock<Game> _mockGame;
        private Mock<Algorithms> _mockAlgorithms;
        private Mock<ChessBoard> _mockChessBoard;
        private Mock<Move> _mockMove;

        [SetUp]
        public void Setup()
        {
            // Assuming Game has a constructor that takes an int
            _mockGame = new Mock<Game>(new object[] { 1 }); // Pass constructor arguments here
            _mockAlgorithms = new Mock<Algorithms>(2);
            _mockChessBoard = new Mock<ChessBoard>();
            _mockMove = new Mock<Move>();

            var mockGames = new ConcurrentDictionary<int, Game>();
            mockGames[1] = _mockGame.Object;

            var mockAlgorithmsDict = new ConcurrentDictionary<int, Algorithms>();
            mockAlgorithmsDict[1] = _mockAlgorithms.Object;

            _gameService = new GameService();

            // Override the internal dictionaries
            typeof(GameService).GetField("_games", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameService, mockGames);
            typeof(GameService).GetField("_gameAlgorithms", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameService, mockAlgorithmsDict);
        }


        // Test InitializeGame
        [Test]
        public void InitializeGame_Should_Return_Valid_Game_Id()
        {
            // Arrange
            var gameId = _gameService.InitializeGame();

            // Act
            Assert.AreEqual(1, gameId); // Assuming it's the first available game ID
        }

        // Test GetAllPlayerMoves
        [Test]
        public void GetAllPlayerMoves_Should_Return_Valid_Moves()
        {
            // Arrange
            int gameId = 1;
            var moves = new List<Move>
            {
                new Move(new Position(0, 1), new Position(0, 3)),
                new Move(new Position(1, 0), new Position(2, 1))
            };
            _mockGame.Setup(g => g.chessBoard.GetAllPlayerMoves(ChessGame.Color.White)).Returns(moves);

            // Act
            var result = _gameService.GetAllPlayerMoves(gameId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(moves.Count, result.Count);
        }

        // Test MakeSentMove
        [Test]
        public void MakeSentMove_Should_Update_Board()
        {
            // Arrange
            int gameId = 1;
            string move = "e2e4"; // Move format for the test
            var start = new Position(0, 1); // E2 position
            var end = new Position(0, 3); // E4 position

            _mockGame.Setup(g => g.ReceiveMove(start, end));

            // Act
            _gameService.MakeSentMove(gameId, move);

            // Assert
            _mockGame.Verify(g => g.ReceiveMove(start, end), Times.Once); // Ensure the move was received by the game
        }

        // Test CalculateComputerMove
        [Test]
        public void CalculateComputerMove_Should_Return_Valid_Move()
        {
            // Arrange
            int gameId = 1;
            var move = new Move(new Position(2, 2), new Position(3, 3)); // Sample move
            _mockAlgorithms.Setup(a => a.Negamax(It.IsAny<ChessBoard>(), It.IsAny<int>(), ChessGame.Color.Black, It.IsAny<int>(), It.IsAny<int>()))
                .Returns((0, move));

            // Act
            var result = _gameService.CalculateComputerMove(gameId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(move, result);
        }

        // Test WhoToMove
        [Test]
        public void WhoToMove_Should_Return_Valid_Player()
        {
            // Arrange
            int gameId = 1;
            _mockGame.Setup(g => g.player).Returns(0); // Assume player 0 is to move

            // Act
            var result = _gameService.WhoToMove(gameId);

            // Assert
            Assert.AreEqual(0, result); // Ensure player 0 is returned
        }

        // Test SendFen
        [Test]
        public void SendFen_Should_Return_Valid_Fen()
        {
            // Arrange
            int gameId = 1;
            string expectedFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // Standard initial FEN
            _mockGame.Setup(g => g.chessBoard.GenerateFEN()).Returns(expectedFen);

            // Act
            var result = _gameService.SendFen(gameId);

            // Assert
            Assert.AreEqual(expectedFen, result);
        }
    }
}
