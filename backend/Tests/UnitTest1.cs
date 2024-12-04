using Chess.net.Services;
using Chess.net.Services.Interfaces;
using Domain.Users;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

public class FriendServiceTests
{
    private readonly Mock<IDataRepository> _mockRepository;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly IFriendService _friendService;

    public FriendServiceTests()
    {
        _mockRepository = new Mock<IDataRepository>();

        // Mock UserManager<User>
        var store = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            store.Object, null, null, null, null, null, null, null, null);

        _friendService = new FriendService(_mockRepository.Object, _mockUserManager.Object);
    }

    [Fact]
    public async Task AddFriend_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((User)null);
        _mockUserManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(new User { Id = "2" });

        // Act
        var result = await _friendService.AddFriend("1", "2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddFriend_ReturnsFalse_WhenFriendNotFound()
    {
        // Arrange
        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(new User { Id = "1" });
        _mockUserManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync((User)null);

        // Act
        var result = await _friendService.AddFriend("1", "2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddFriend_ReturnsFalse_WhenFriendshipAlreadyExists()
    {
        // Arrange
        var friendship = new Friend { UserId = "1", FriendId = "2" };
        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(new User { Id = "1" });
        _mockUserManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(new User { Id = "2" });
        _mockRepository.Setup(r => r.FriendRepository.GetAllAsync())
            .ReturnsAsync(new List<Friend> { friendship });

        // Act
        var result = await _friendService.AddFriend("1", "2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddFriend_ReturnsTrue_WhenFriendshipAddedSuccessfully()
    {
        // Arrange
        _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(new User { Id = "1" });
        _mockUserManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(new User { Id = "2" });
        _mockRepository.Setup(r => r.FriendRepository.GetAllAsync())
            .ReturnsAsync(new List<Friend>());
        _mockRepository.Setup(r => r.FriendRepository.AddAsync(It.IsAny<Friend>()))
            .ReturnsAsync(1);

        // Act
        var result = await _friendService.AddFriend("1", "2");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ListAllFriends_ReturnsEmptyList_WhenNoFriendsExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.FriendRepository.GetAllAsync())
            .ReturnsAsync(new List<Friend>());

        // Act
        var result = await _friendService.ListAllFriends("1");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAllFriends_ReturnsListOfFriends()
    {
        // Arrange
        var friendships = new List<Friend>
        {
            new Friend { UserId = "1", FriendId = "2" },
            new Friend { UserId = "3", FriendId = "1" }
        };

        var users = new List<User>
        {
            new User { Id = "2", UserName = "Friend1" },
            new User { Id = "3", UserName = "Friend2" }
        };

        _mockRepository.Setup(r => r.FriendRepository.GetAllAsync())
            .ReturnsAsync(friendships);
        _mockUserManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(users[0]);
        _mockUserManager.Setup(m => m.FindByIdAsync("3")).ReturnsAsync(users[1]);

        // Act
        var result = await _friendService.ListAllFriends("1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == "2");
        Assert.Contains(result, u => u.Id == "3");
    }
}
