using Moq;
using Logic.Services;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using LogicTests;

namespace Logic.Tests
{
   // [TestFixture]
    //public abstract class BaseAccountServiceTest
    //{
    //    protected Mock<UserManager<User>> _userManager;
    //    protected Mock<SignInManager<User>> _signInManager;
    //    protected IConfiguration _configuration;
    //    protected AccountService _accountService;

    //    [SetUp]
    //    public void BaseSetUp()
    //    {
    //        _userManager = TestHelpers.MockUserManager();

    //        _signInManager = TestHelpers.MockSignInManager(_userManager);

    //        // Ustawienie konfiguracji dla JWT
    //        var inMemorySettings = new Dictionary<string, string?> {
    //            {"Jwt:Key", "supersecretkeyasdasdq231234532ewsd"},
    //            {"Jwt:Issuer", "issuer"},
    //            {"Jwt:Audience", "audience"}
    //        };

    //        _configuration = new ConfigurationBuilder()
    //            .AddInMemoryCollection(inMemorySettings)
    //            .Build();

    //        _accountService = new AccountService(_userManager.Object, _signInManager.Object, _configuration);
    //    }
    //}
}
