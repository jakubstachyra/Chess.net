using Chess.net.Controllers;
using Chess.net.Services;
using Domain.Users;
using Infrastructure.DataContext;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private DomainDataContext _context;
        private IAdminService _adminService;
        private AdminController _adminController;
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<DomainDataContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;
            _context = new DomainDataContext(options);

            var serviceCollection = new ServiceCollection();

            // Rejestracja zależności
            serviceCollection.AddSingleton(_context);
            serviceCollection.AddSingleton<IUserStore<User>>(new UserStore<User>(_context));
            serviceCollection.AddSingleton<IRoleStore<IdentityRole>>(new RoleStore<IdentityRole>(_context));

            serviceCollection.AddSingleton<UserManager<User>>(provider =>
            {
                var userStore = provider.GetRequiredService<IUserStore<User>>();
                return new UserManager<User>(
                    userStore, null, new PasswordHasher<User>(), null, null, null, null, null, null
                );
            });

            serviceCollection.AddSingleton<RoleManager<IdentityRole>>(provider =>
            {
                var roleStore = provider.GetRequiredService<IRoleStore<IdentityRole>>();
                return new RoleManager<IdentityRole>(
                    roleStore, null, null, null, null
                );
            });

            serviceCollection.AddSingleton<IAdminService, AdminService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _adminService = serviceProvider.GetRequiredService<IAdminService>();
            _adminController = new AdminController(_adminService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task BanUser_ShouldBanUser_WhenUserExists()
        {
            var user = new User { Id = "1", UserName = "TestUser", IsBanned = false };
            await _userManager.CreateAsync(user);

            var result = await _adminController.BanUser(user.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var updatedUser = await _userManager.FindByIdAsync(user.Id);
            Assert.IsNotNull(updatedUser);
            Assert.IsTrue(updatedUser.IsBanned);
        }

        [Test]
        public async Task BanUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
  
            var result = await _adminController.BanUser("45");

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task MakeAdmin_ShouldAddAdminRole_WhenUserExists()
        {
            var user = new User { Id = "1", UserName = "Ralph", IsBanned = false };
            await _userManager.CreateAsync(user);

            var result = await _adminService.MakeAdmin(user.Id);

            Assert.IsTrue(result);

            var roles = await _userManager.GetRolesAsync(user);
            Assert.Contains("Admin", roles.ToList());
        }

        [Test]
        public async Task MakeAdmin_ShouldCreateAdminRole_WhenRoleDoesNotExist()
        {
            var user = new User { Id = "1", UserName = "Ralph", IsBanned = false };
            await _userManager.CreateAsync(user);

            var roleExists = await _roleManager.RoleExistsAsync("Admin");
            Assert.IsFalse(roleExists);

            var result = await _adminService.MakeAdmin(user.Id);

            Assert.IsTrue(result);

            roleExists = await _roleManager.RoleExistsAsync("Admin");
            Assert.IsTrue(roleExists);

            var roles = await _userManager.GetRolesAsync(user);
            Assert.Contains("Admin", roles.ToList());
        }

        [Test]
        public async Task MakeAdmin_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var result = await _adminService.MakeAdmin("999");

            Assert.IsFalse(result);
        }
    }
}
