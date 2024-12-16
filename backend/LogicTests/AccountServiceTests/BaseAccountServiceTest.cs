using NUnit.Framework;
using Moq;
using Logic.Services;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using LogicTests;

namespace Logic.Tests
{
    public abstract class BaseAccountServiceTest
    {
        protected Mock<UserManager<User>> _userManager;
        protected Mock<SignInManager<User>> _signInManager;
        protected Mock<IConfiguration> _configuration;
        protected AccountService _accountService;

        [SetUp]
        public void BaseSetUp()
        {
            _userManager = TestHelpers.MockUserManager();

            _signInManager = TestHelpers.MockSignInManager(_userManager);

            _configuration = new Mock<IConfiguration>();
            _accountService = new AccountService(_userManager.Object, _signInManager.Object, _configuration.Object);
        }
    }
}
