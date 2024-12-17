using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ApiTests.AccountControllerTests
{
    [TestFixture]
    public class AccountControllerBaseTests
    {
        protected Mock<IAccountService> _accountService;
        protected AccountController _accountController;

        [SetUp]
        public void SetUp()
        {
            _accountService = new Mock<IAccountService>();
            _accountController = new AccountController(_accountService.Object);
        }

    }
}
