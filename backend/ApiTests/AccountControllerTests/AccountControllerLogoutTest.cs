
using Microsoft.AspNetCore.Mvc;
using static AccountController;

namespace ApiTests.AccountControllerTests
{
    public class AccountControllerLogoutTest : AccountControllerBaseTests
    {
        [Test]
        public void Logout_DeletesCookieAndReturnsOk()
        {
            var result = _accountController.Logout();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var message = okResult.Value as LogOutResponse;
            Assert.That(message!.Message, Is.EqualTo("Logged out successfully."));
        }
    }
}
