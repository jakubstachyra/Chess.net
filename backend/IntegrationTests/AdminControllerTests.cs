using Chess.net.Controllers;
using Chess.net.Services;
using Chess.net.Services.Interfaces;
using Domain.Users;
using Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IntegrationTests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private DomainDataContext _context;
        private IAdminService _adminService;
        private AdminController _adminController;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<DomainDataContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;
            _context = new DomainDataContext(options);

            var serviceCollection = new ServiceCollection();

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
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _adminController.BanUser(user.Id);

            Assert.IsInstanceOf<OkObjectResult>(result);

            var updatedUser = await _context.Users.FindAsync(user.Id);

            Assert.IsNotNull(updatedUser);
            Assert.IsTrue(updatedUser.IsBanned);
        }
    }
}
