using Chess.net.Services;
using NUnit.Framework;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Moq;


namespace LogicTests
{
    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private AdminService _adminService;
        [SetUp]
        public void Setup()
        {
            _mockRoleManager = MockRoleManager();
            _mockUserManager = MockUserManager();
            _adminService = new AdminService(_mockUserManager.Object, _mockRoleManager.Object);
        }
        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }


        [Test]
        public async Task BanUser_Should_Return_True_WhenUserExists()
        {
            var userId = "testUserId";
            var user = new User { Id = userId };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _adminService.BanUser(userId);

            Assert.IsTrue(result);
            Assert.IsTrue(user.IsBanned);
        }
        [Test]
        public async Task BanUser_Should_Return_False_WhenUserDoesNotExist()
        {
            var userId = "userId";

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _adminService.BanUser(userId);

            Assert.IsFalse(result);
        }
        [Test]
        public async Task MakeAdmin_Should_Return_True_WhenUserExistsAndRoleExists()
        {
            var userId = "userId";
            var user = new User { Id = userId };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockRoleManager.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(true);
            _mockUserManager.Setup(x => x.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

            var result = await _adminService.MakeAdmin(userId);
            Assert.IsTrue(result);
        }
        [Test]
        public async Task MakeAdmin_Should_Return_False_WhenUserDoesNotExist()
        {
            var userId = "userId";

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _adminService.MakeAdmin(userId);

            Assert.IsFalse(result);
        }
/*        [Test]
        public async Task MakeAdmin_Should_Return_False_WhenUserExistsAndRoleDoesNotExist()
        {
            var userId = "userId";
            var user = new User { Id = userId };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockRoleManager.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(false);

            var result = await _adminService.MakeAdmin(userId);

            Assert.IsFalse(result);
        }*/
    }
}
