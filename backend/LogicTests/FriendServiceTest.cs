using NUnit.Framework;
using Moq;
using Chess.net.Services;
using Domain.Users;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogicTests
{
    //[TestFixture]
    //public class FriendServiceTests
    //{
    //    private Mock<UserManager<User>> _mockUserManager;
    //    private Mock<IDataRepository> _mockRepository;
    //    private Mock<IFriendRepository> _mockFriendRepository;
    //    private FriendService _friendService;

    //    [SetUp]
    //    public void Setup()
    //    {
    //        _mockUserManager = TestHelpers.MockUserManager();

    //        _mockFriendRepository = new Mock<IFriendRepository>();

    //        _mockRepository = new Mock<IDataRepository>();
    //        _mockRepository.Setup(r => r.FriendRepository).Returns(_mockFriendRepository.Object);

    //        _friendService = new FriendService(_mockRepository.Object, _mockUserManager.Object);
    //    }


    //    [Test]
    //    public async Task AddFriend_Should_Return_False_When_User_Or_Friend_Is_Null()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        string friendId = "2";

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);
    //        _mockUserManager.Setup(x => x.FindByIdAsync(friendId)).ReturnsAsync((User)null);

    //        // Act
    //        var result = await _friendService.AddFriend(userId, friendId);

    //        // Assert
    //        Assert.IsFalse(result);
    //    }

    //    [Test]
    //    public async Task AddFriend_Should_Return_False_When_User_Adds_Himself()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        var user = new User { Id = userId, UserName = "User" };

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

    //        // Act
    //        var result = await _friendService.AddFriend(userId, userId);

    //        // Assert
    //        Assert.IsFalse(result);
    //    }

    //    [Test]
    //    public async Task AddFriend_Should_Return_True_When_Users_Exists()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        string friendId = "2";
    //        var user = new User { Id = userId, UserName = "User" };
    //        var friend = new User { Id = friendId, UserName = "Friend" };

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
    //        _mockUserManager.Setup(x => x.FindByIdAsync(friendId)).ReturnsAsync(friend);

    //        _mockFriendRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Friend>());
    //        _mockFriendRepository.Setup(r => r.AddAsync(It.IsAny<Friend>())).ReturnsAsync(1);

    //        // Act
    //        var result = await _friendService.AddFriend(userId, friendId);

    //        // Assert
    //        Assert.IsTrue(result);
    //    }

    //    [Test]
    //    public async Task AddFriend_Should_Return_False_When_Friendship_Exists()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        string friendId = "2";

    //        var user = new User { Id = userId, UserName = "User" };
    //        var friend = new User { Id = friendId, UserName = "Friend" };

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
    //        _mockUserManager.Setup(x => x.FindByIdAsync(friendId)).ReturnsAsync(friend);

    //        _mockFriendRepository.Setup(r => r.GetAllAsync())
    //            .ReturnsAsync(new List<Friend> { new Friend { UserId = userId, FriendId = friendId } });

    //        // Act
    //        var result = await _friendService.AddFriend(userId, friendId);

    //        // Assert
    //        Assert.IsFalse(result);
    //    }

    //    [Test]
    //    public async Task AddFriend_Should_Return_False_When_Existing_Friendship_Found()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        string friendId = "2";
    //        var user = new User { Id = userId };
    //        var friend = new User { Id = friendId };

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
    //        _mockUserManager.Setup(x => x.FindByIdAsync(friendId)).ReturnsAsync(friend);
    //        _mockFriendRepository.Setup(r => r.GetAllAsync())
    //            .ReturnsAsync(new List<Friend>
    //            {
    //                new Friend { UserId = userId, FriendId = friendId }
    //            });

    //        // Act
    //        var result = await _friendService.AddFriend(userId, friendId);
    //        var resultSecond = await _friendService.AddFriend(friendId, userId);

    //        // Assert
    //        Assert.IsFalse(result);
    //        Assert.IsFalse(resultSecond);
    //    }

    //    [Test]
    //    public async Task ListAllFriends_Should_Return_Empty_List_When_No_Friends()
    //    {
    //        // Arrange
    //        string userId = "1";

    //        _mockFriendRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Friend>());

    //        // Act
    //        var result = await _friendService.ListAllFriends(userId);

    //        // Assert
    //        Assert.IsNotNull(result);
    //        Assert.IsEmpty(result);
    //    }

    //    [Test]
    //    public async Task ListAllFriends_Should_Return_Friends_List_When_Friends_Exist()
    //    {
    //        // Arrange
    //        string userId = "1";
    //        string friendId = "2";
    //        var user = new User { Id = userId };
    //        var friendUser = new User { Id = friendId };

    //        _mockFriendRepository.Setup(r => r.GetAllAsync())
    //            .ReturnsAsync(new List<Friend>
    //            {
    //                new Friend { UserId = userId, FriendId = friendId }
    //            });

    //        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
    //        _mockUserManager.Setup(x => x.FindByIdAsync(friendId)).ReturnsAsync(friendUser);

    //        // Act
    //        var result = await _friendService.ListAllFriends(userId);
    //        var resultFriend = await _friendService.ListAllFriends(friendId);

    //        // Assert
    //        Assert.IsNotNull(result);
    //        Assert.AreEqual(1, result.Count);
    //        Assert.AreEqual(friendId, result[0].Id);

    //        Assert.IsNotNull(resultFriend);
    //        Assert.AreEqual(1, resultFriend.Count);
    //        Assert.AreEqual(userId, resultFriend[0].Id);
    //    }
    //}
}
