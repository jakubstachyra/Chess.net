using Chess.net.Controllers;
using Chess.net.Services;
using Domain.AuthModels;
using Domain.Users;
using Infrastructure.DataContext;
using Logic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Logic.Interfaces;

namespace IntegrationTests
{
//    [TestFixture]
//    public class AccountServiceTests
//    {
//        private DomainDataContext _context;
//        private IAccountService _accountService;
//        private AccountController _accountController;
//        private UserManager<User> _userManager;
//        private SignInManager<User> _signInManager;
//        private RoleManager<IdentityRole> _roleManager;
//        private IConfiguration _configuration;
//        [SetUp]
//        public void SetUp()
//        {
//            var options = new DbContextOptionsBuilder<DomainDataContext>()
//                .UseInMemoryDatabase("TestDatabase")
//                .Options;
//            _context = new DomainDataContext(options);

//            var serviceCollection = new ServiceCollection();

//            // Rejestracja UserStore i RoleStore
//            serviceCollection.AddSingleton<IUserStore<User>>(new UserStore<User>(_context));
//            serviceCollection.AddSingleton<IRoleStore<IdentityRole>>(new RoleStore<IdentityRole>(_context));

//            // Rejestracja UserManager i RoleManager
//            serviceCollection.AddSingleton<UserManager<User>>(provider =>
//            {
//                var userStore = provider.GetRequiredService<IUserStore<User>>();
//                return new UserManager<User>(
//                    userStore, null, new PasswordHasher<User>(), null, null, null, null, null, null
//                );
//            });

//            serviceCollection.AddSingleton<RoleManager<IdentityRole>>(provider =>
//            {
//                var roleStore = provider.GetRequiredService<IRoleStore<IdentityRole>>();
//                return new RoleManager<IdentityRole>(
//                    roleStore, null, null, null, null
//                );
//            });

//            // Rejestracja IHttpContextAccessor
//            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//            // Rejestracja SignInManager
//            serviceCollection.AddSingleton<SignInManager<User>>(provider =>
//            {
//                var userManager = provider.GetRequiredService<UserManager<User>>();
//                var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
//                var claimsFactory = Mock.Of<IUserClaimsPrincipalFactory<User>>();

//                return new SignInManager<User>(
//                    userManager, contextAccessor, claimsFactory, null, null, null, null
//                );
//            });

//            // Dodanie konfiguracji JWT (InMemory)
//            var inMemorySettings = new Dictionary<string, string>
//            {
//                { "Jwt:Key", "SuperSecretKey123asda3sadxcz23894798342s45" },
//                { "Jwt:Issuer", "TestIssuer" },
//                { "Jwt:Audience", "TestAudience" }
//            };
//            _configuration = new ConfigurationBuilder()
//                .AddInMemoryCollection(inMemorySettings)
//                .Build();

//            serviceCollection.AddSingleton<IConfiguration>(_configuration);

//            // Rejestracja AccountService
//            serviceCollection.AddSingleton<IAccountService, AccountService>();

//            // Budowanie ServiceProvider
//            var serviceProvider = serviceCollection.BuildServiceProvider();

//            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
//            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//            _signInManager = serviceProvider.GetRequiredService<SignInManager<User>>();
//            _accountService = new AccountService(_userManager, _signInManager, _configuration);

//            // Utworzenie instancji AccountController
//            _accountController = new AccountController(_accountService);
//        }


//        [TearDown]
//        public void TearDown()
//        {
//            _context.Database.EnsureDeleted();
//            _context.Dispose();
//        }

//        [Test]
//        public async Task RegisterUser_ShouldRegisterUser_WhenDataIsValid()
//        {
//            // Arrange
//            var registerModel = new RegisterModel
//            {
//                Username = "TestUser",
//                Email = "test@example.com",
//                Password = "Password123!",
//                ConfirmPassword = "Password123!",
//            };

//            if (!await _roleManager.RoleExistsAsync("USER"))
//            {
//                await _roleManager.CreateAsync(new IdentityRole("USER"));
//            }

//            // Act
//            var (success, errors) = await _accountService.RegisterUser(registerModel);

//            // Assert
//            Assert.IsTrue(success);
//            Assert.IsEmpty(errors);

//            var user = await _userManager.FindByEmailAsync(registerModel.Email);
//            Assert.IsNotNull(user);
//            Assert.AreEqual(registerModel.Username, user.UserName);

//            var roles = await _userManager.GetRolesAsync(user);
//            Assert.Contains("USER", roles.ToList());
//        }

//        [Test]
//        public async Task RegisterUser_ShouldSucceed_WhenEmailIsUnique()
//        {
//            // Arrange
//            var registerModel = new RegisterModel
//            {
//                Username = "TestUser",
//                Email = "test@example.com",
//                Password = "Password123!",
//                ConfirmPassword = "Password123!",
//            };

//            // Usuń użytkownika, jeśli już istnieje (reset bazy danych)
//            var user = await _userManager.FindByEmailAsync(registerModel.Email);
//            if (user != null)
//            {
//                await _userManager.DeleteAsync(user);
//            }

//            // Upewnij się, że rola 'USER' istnieje
//            if (!await _roleManager.RoleExistsAsync("USER"))
//            {
//                await _roleManager.CreateAsync(new IdentityRole("USER"));
//            }

//            // Act
//            var (success, errors) = await _accountService.RegisterUser(registerModel);

//            // Assert
//            Assert.IsTrue(success);
//            Assert.IsEmpty(errors);
//        }



//        [Test]
//       public async Task LoginUser_ShouldReturnToken_WhenCredentialsAreValid()
//        {
//            // Arrange
//            var user = new User { UserName = "TestUser", Email = "test@example.com" };
//            await _userManager.CreateAsync(user, "Password123!");

//            var loginModel = new LoginModel
//            {
//                Email = "test@example.com",
//                Password = "Password123!"
//            };

//            // Act
//            var (success, token, errors) = await _accountService.LoginUser(loginModel);

//            // Assert
//            Assert.IsTrue(success);
//            Assert.IsNotNull(token);
//            Assert.IsEmpty(errors);
//        }
//        /*[Test]
//        public async Task LoginUser_ShouldFail_WhenPasswordIsInvalid()
//        {
//            // Arrange
//            var user = new User { UserName = "TestUser", Email = "test@example.com" };
//            await _userManager.CreateAsync(user, "Password123!");

//            // Konfiguracja loggera jako NullLogger
//            var logger = NullLogger<AccountService>.Instance;

//            // Utworzenie AccountService z loggerem
//            var accountService = new AccountService(_userManager, _roleManager, logger);

//            var loginModel = new LoginModel
//            {
//                Email = "test@example.com",
//                Password = "WrongPassword!"
//            };

//            // Act
//            var (success, token, errors) = await accountService.LoginUser(loginModel);

//            // Assert
//            Assert.IsFalse(success);
//            Assert.IsNull(token);
//            Assert.IsNotEmpty(errors);
//            Assert.IsTrue(errors.Any(e => e.Description.Contains("incorrect")));
//        }
//*/
//        [Test]
//        public async Task LoginUser_ShouldFail_WhenUserDoesNotExist()
//        {
//            // Arrange
//            var loginModel = new LoginModel
//            {
//                Email = "nonexistent@example.com",
//                Password = "Password123!"
//            };

//            // Act
//            var (success, token, errors) = await _accountService.LoginUser(loginModel);

//            // Assert
//            Assert.IsFalse(success);
//            Assert.IsNull(token);
//            Assert.IsNotEmpty(errors);
//            Assert.IsTrue(errors.Any(e => e.Description.Contains("does not exist")));
//        }
//    }
}
