using Chess.net.Services;
using Domain.Common;
using Domain.Users;
using Infrastructure.Interfaces;
using Moq;

namespace LogicTests
{
    public class SaveGameServiceTests
    {
    //    private Mock<IDataRepository> _mockRepository;
    //    private Mock<IGameRepository> _mockGameRepository;
    //    private Mock<IMoveRepository> _mockMoveRepository;
    //    private SaveGameService _saveGameService;

    //    [SetUp]
    //    public void Setup()
    //    {

    //        _mockRepository = new Mock<IDataRepository>();
    //        _mockGameRepository = new Mock<IGameRepository>();
    //        _mockMoveRepository = new Mock<IMoveRepository>();

    //        _mockRepository.Setup(r => r.GameRepository).Returns(_mockGameRepository.Object);
    //        _mockRepository.Setup(r => r.MoveRepository).Returns(_mockMoveRepository.Object);

    //        _saveGameService = new SaveGameService(_mockRepository.Object);
    //    }

    //    [Test]
    //    public async Task SaveMovesAsync_Should_Return_False_When_Game_Not_Found()
    //    {
    //        // Arrange
    //        int gameId = 1;
    //        List<ChessGame.GameMechanics.Move> whiteMoves = new List<ChessGame.GameMechanics.Move>
    //    {
    //        new ChessGame.GameMechanics.Move("e2", "e4")
    //    };
    //        List<ChessGame.GameMechanics.Move> blackMoves = new List<ChessGame.GameMechanics.Move>
    //    {
    //        new ChessGame.GameMechanics.Move("e7", "e5")
    //    };
    //        List<long> whiteTimeMs = new List<long> { 5000 };
    //        List<long> blackTimeMs = new List<long> { 5000 };

    //        _mockGameRepository.Setup(g => g.GetByIDAsync(gameId)).ReturnsAsync((Game)null);

    //        // Act
    //        var result = await _saveGameService.SaveMovesAsync(gameId, whiteMoves, blackMoves, whiteTimeMs, blackTimeMs);

    //        // Assert
    //        Assert.IsFalse(result);
    //    }

    //    [Test]
    //    public async Task SaveMovesAsync_Should_Return_True_When_Moves_Are_Saved()
    //    {
    //        // Arrange
    //        int gameId = 1;
    //        List<ChessGame.GameMechanics.Move> whiteMoves = new List<ChessGame.GameMechanics.Move>
    //{
    //    new ChessGame.GameMechanics.Move(ChessGame.Utils.Converter.ChessNotationToPosition("e2"), ChessGame.Utils.Converter.ChessNotationToPosition("e4"))
    //};
    //        List<ChessGame.GameMechanics.Move> blackMoves = new List<ChessGame.GameMechanics.Move>
    //{
    //    new ChessGame.GameMechanics.Move(ChessGame.Utils.Converter.ChessNotationToPosition("e7"), ChessGame.Utils.Converter.ChessNotationToPosition("e5"))
    //};
    //        List<long> whiteTimeMs = new List<long> { 5000 };
    //        List<long> blackTimeMs = new List<long> { 5000 };

    //        var user1 = new User { Id = "1", UserName = "User1" };
    //        var user2 = new User { Id = "2", UserName = "User2" };
    //        var gameMode = new GameMode { Description = "Classic" };
    //        var game = new Game { Id = gameId, WhitePlayer = user1, BlackPlayer = user2, GameMode = gameMode };

    //        _mockGameRepository.Setup(g => g.GetByIDAsync(gameId)).ReturnsAsync(game);

    //        // Act
    //        var result = await _saveGameService.SaveMovesAsync(gameId, whiteMoves, blackMoves, whiteTimeMs, blackTimeMs);

    //        // Assert
    //        Assert.IsTrue(result);
    //        _mockMoveRepository.Verify(m => m.AddAsync(It.IsAny<Move>()), Times.Exactly(1));
    //        _mockMoveRepository.Verify(m => m.AddAsync(It.Is<Move>(move => move.WhiteMove == "e2 e4" && move.BlackMove == "e7 e5")), Times.Once());

    //    }

    //    public async Task SaveMovesAsync_Should_Save_White_Move_When_More_White_Moves()
    //    {
    //        // Arrange
    //        int gameId = 1;
    //        List<ChessGame.GameMechanics.Move> whiteMoves = new List<ChessGame.GameMechanics.Move>
    //{
    //    new ChessGame.GameMechanics.Move(ChessGame.Utils.Converter.ChessNotationToPosition("e2"), ChessGame.Utils.Converter.ChessNotationToPosition("e4"))
    //,        new ChessGame.GameMechanics.Move(ChessGame.Utils.Converter.ChessNotationToPosition("d2"), ChessGame.Utils.Converter.ChessNotationToPosition("d4"))

    //    };
    //        List<ChessGame.GameMechanics.Move> blackMoves = new List<ChessGame.GameMechanics.Move>
    //{
    //    new ChessGame.GameMechanics.Move(ChessGame.Utils.Converter.ChessNotationToPosition("e7"), ChessGame.Utils.Converter.ChessNotationToPosition("e5"))
    //};

    //        List<long> whiteTimeMs = new List<long> { 5000 };
    //        List<long> blackTimeMs = new List<long> { 5000 };

    //        var user1 = new User { Id = "1", UserName = "User1" };
    //        var user2 = new User { Id = "2", UserName = "User2" };
    //        var gameMode = new GameMode { Description = "Classic" };
    //        var game = new Game { Id = gameId, WhitePlayer = user1, BlackPlayer = user2, GameMode = gameMode };

    //        _mockGameRepository.Setup(g => g.GetByIDAsync(gameId)).ReturnsAsync(game);

    //        // Act
    //        var result = await _saveGameService.SaveMovesAsync(gameId, whiteMoves, blackMoves, whiteTimeMs, blackTimeMs);

    //        // Assert
    //        Assert.IsTrue(result);

    //        // Assert
    //        Assert.IsTrue(result);
    //        _mockMoveRepository.Verify(m => m.AddAsync(It.IsAny<Move>()), Times.Exactly(2));
    //        _mockMoveRepository.Verify(m => m.AddAsync(It.Is<Move>(move => move.WhiteMove == "e2 e4" && move.BlackMove == "d2 d4")), Times.Once());
    //        _mockMoveRepository.Verify(m => m.AddAsync(It.Is<Move>(move => move.BlackMove == "e7 e5" && move.BlackMove == null)), Times.Once());
    //    }

    //    [Test]
    //    public async Task ReturnMovesAsync_Should_Return_Moves_When_Exist()
    //    {
    //        // Arrange
    //        int gameId = 1;
    //        var user1 = new User { Id = "1", UserName = "User1" };
    //        var user2 = new User { Id = "2", UserName = "User2" };
    //        var gameMode = new GameMode { Description = "Classic" };
    //        var game = new Game { Id = gameId, WhitePlayer = user1, BlackPlayer = user2, GameMode = gameMode };

    //        var moves = new List<Move>
    //        {
    //            new Move { WhiteMove = "e2 e4", BlackMove = "e7 e5", MoveNumber = 1, Game=game },
    //            new Move { WhiteMove = "e4 e5", BlackMove = "e5 e6", MoveNumber = 2, Game= game }
    //        };

    //        _mockMoveRepository.Setup(m => m.GetByConditionAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Move, bool>>>())).ReturnsAsync(moves);

    //        // Act
    //        var result = await _saveGameService.returnMovesAsync(gameId);

    //        // Assert
    //        Assert.IsNotNull(result);
    //        Assert.AreEqual(2, result.whiteMoves.Count);
    //        Assert.AreEqual(2, result.blackMoves.Count);
    //        Assert.AreEqual("e2 e4", result.whiteMoves[0]);
    //        Assert.AreEqual("e5 e6", result.blackMoves[1]);
    //    }

    }
}