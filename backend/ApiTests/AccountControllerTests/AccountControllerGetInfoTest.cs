using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ApiTests.AccountControllerTests
{
    public class AccountControllerGetInfoTest :AccountControllerBaseTests
    {
        [Test]
        public async Task GetUserInfo_ReturnsOk_WithUserDetails()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "testuser")
            }));

            _accountService.Setup(s => s.GetUserInfo(claimsPrincipal))
                               .ReturnsAsync(("test@example.com", "testuser"));

            // Act
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            _accountController.ControllerContext = controllerContext;

            var result = await _accountController.GetUserInfo();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var response = okResult.Value as dynamic;
            Assert.AreEqual("test@example.com", response.Email);
            Assert.AreEqual("testuser", response.Username);
        }
    }
}
