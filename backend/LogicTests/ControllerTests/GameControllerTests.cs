using Chess.net.Controllers;
using ChessGame;
using Moq;
using NUnit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ChessGame.GameMechanics;
using NuGet.Packaging;
using Logic.Interfaces;



[TestFixture]
    public class GameControllerTests
    {
        private Mock<IGameService> _gameServiceMock;
        private GameController _gameController;

        [SetUp]
        public void SetUp()
        {
            _gameServiceMock = new Mock<IGameService>();
            _gameController = new GameController(_gameServiceMock.Object);
        }

    [Test]
    public void CreateGame_ShouldReturnGameId()
    {
        // Arrange
        int expectedGameId = 1;
        _gameServiceMock.Setup(service => service.InitializeGame()).Returns(expectedGameId);

        // Act
        var result = _gameController.CreateGame();

        // Assert
        Assert.AreEqual(expectedGameId.ToString(), result);
        _gameServiceMock.Verify(service => service.InitializeGame(), Times.Once);
    }

    [Test]
    public void CreateGame_ShouldReturnDifferentIdsOnMultipleCalls()
    {
        // Arrange
        _gameServiceMock
            .SetupSequence(service => service.InitializeGame())
            .Returns(1) 
            .Returns(2);

        // Act
        var firstResult = _gameController.CreateGame(); // Pierwsze wywołanie
        var secondResult = _gameController.CreateGame(); // Drugie wywołanie

        // Assert
        Assert.AreEqual("1", firstResult, "Pierwsze wywołanie nie zwróciło oczekiwanego ID.");
        Assert.AreEqual("2", secondResult, "Drugie wywołanie nie zwróciło oczekiwanego ID.");
        _gameServiceMock.Verify(service => service.InitializeGame(), Times.Exactly(2));
    }

    [Test]
    public void CreateGame_ShouldHandleMultipleCases()
    {
        // Arrange
        int[] testGameIds = { 1, 42, 0, -1 }; // Różne zwracane wartości, w tym nietypowe
        foreach (var expectedGameId in testGameIds)
        {
            _gameServiceMock.Setup(service => service.InitializeGame()).Returns(expectedGameId);

            // Act
            var result = _gameController.CreateGame();

            // Assert
            Assert.AreEqual(expectedGameId.ToString(), result);
            _gameServiceMock.Verify(service => service.InitializeGame(), Times.Once);

            // Reset mock after each iteration
            _gameServiceMock.Reset();
        }

        // Arrange for exception case
        _gameServiceMock.Setup(service => service.InitializeGame())
            .Throws(new InvalidOperationException("Initialization failed"));

        // Act & Assert for exception
        Assert.Throws<InvalidOperationException>(() => _gameController.CreateGame());
    }


/*    [Test]
    public void GetMoves_ShouldReturnListOfMoves()
    {
        // Arrange
        int gameId = 1;
        var moves = new List<Move>
    {
        new Move("e2", "e4"),
        new Move("e7", "e5"),
        new Move("a2", "a4")
    };

        _gameServiceMock
            .Setup(service => service.GetAllPlayerMoves(gameId))
            .Returns(moves);

        // Act
        var result = _gameController.GetMoves(gameId);

        // Assert
        Assert.AreEqual(moves.Count, result.Count, "Ilość ruchów jest różna.");
        for (int i = 0; i < moves.Count; i++)
        {
            Assert.AreEqual(moves[i].from, result[i].from, $"Ruch {i}: Początkowe pole różni się.");
            Assert.AreEqual(moves[i].to, result[i].to, $"Ruch {i}: Końcowe pole różni się.");
            Console.WriteLine($"{moves[i].from}  {moves[i].to}"); 
        }

        _gameServiceMock.Verify(service => service.GetAllPlayerMoves(gameId), Times.Once);
    }*/

    //[Test]
    //public void GetMoves_ShouldReturnListOfMoves()
    //{
    //    // Arrange
    //    int gameId = 1;
    //    var moves = new List<Move>
    //{
    //    new Move("e2", "e4"),
    //    new Move("e7", "e5"),
    //    new Move("a2", "a4")
    //};

    //    _gameServiceMock
    //        .Setup(service => service.GetAllPlayerMoves(gameId))
    //        .Returns(moves);

    //    // Act
    //    var result = _gameController.GetMoves(gameId);

    //    // Assert
    //    Assert.AreEqual(moves.Count, result.Count, "Ilość ruchów jest różna.");
    //    for (int i = 0; i < moves.Count; i++)
    //    {
    //        Assert.AreEqual(moves[i].from, result[i].from, $"Ruch {i}: Początkowe pole różni się.");
    //        Assert.AreEqual(moves[i].to, result[i].to, $"Ruch {i}: Końcowe pole różni się.");
    //        Console.WriteLine($"{moves[i].from}  {moves[i].to}"); 
    //    }

    //    _gameServiceMock.Verify(service => service.GetAllPlayerMoves(gameId), Times.Once);
    //}



}
