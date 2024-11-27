using Chess.net.Services;
using Chess.net.Services.Interfaces;
using Domain.Users;
using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class FriendServiceTests
{
    //private Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    //{
    //    var queryableData = data.AsQueryable();
    //    var mockSet = new Mock<DbSet<T>>();

    //    mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
    //    mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
    //    mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
    //    mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

    //    mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>((entity) => ((List<T>)data).Add(entity));

    //    return mockSet;
    //}

    //[Fact]
    //public async Task AddFriend_ShouldReturnTrue_WhenValid()
    //{
    //    // Arrange
    //    var users = new List<User>
    //    {
    //        new User { Id = "1"},
    //        new User { Id = "2"},
    //        new User { Id = "3" }
    //    };

    //    var friends = new List<Friend>();

    //    var mockUsersDbSet = CreateMockDbSet(users);
    //    var mockFriendsDbSet = CreateMockDbSet(friends);

    //    var mockContext = new Mock<DomainDataContext>();
    //    mockContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);
    //    mockContext.Setup(c => c.Friends).Returns(mockFriendsDbSet.Object);

    //    var service = new FriendService(mockContext.Object);

    //    // Act
    //    var result = await service.AddFriend(1, 3);

    //    // Assert
    //    Assert.True(result);
    //    Assert.Single(friends, f => f.UserId == 1 && f.FriendId == 3);
    //}

  
}
