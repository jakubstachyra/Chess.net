using Domain.AuthModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static AccountController;

namespace ApiTests.AccountControllerTests
{
    //public class AccountControllerLogInTests : AccountControllerBaseTests
    //{
       // [Test]
      //  public async Task Login_ReturnsUnauthorized_WhenLoginFails()
      //{
    //        var loginModel = new LoginModel { Email = "test@example.com", Password = "wrongpassword" };
    //        _accountService.Setup(s => s.LoginUser(loginModel))
    //                           .ReturnsAsync((false, null, new List<IdentityError>
    //                           {
    //                               new IdentityError { Code = "InvalidPassword", Description = "Wrong password" }
    //                           }));
    //        var result = await _accountController.Login(loginModel);

    //        var unauthorizedResult = result as UnauthorizedObjectResult;
    //        Assert.IsNotNull(unauthorizedResult);
    //        Assert.That(unauthorizedResult.StatusCode, Is.EqualTo(401));

    //        var errors = unauthorizedResult.Value as IEnumerable<IdentityError>;
    //        Assert.That(errors, Is.Not.Empty);
    //    }

    //    [Test]
    //    public async Task Login_ReturnsOk_WithToken_WhenLoginIsSuccessful()
    //    {
    //        // Arrange
    //        var loginModel = new LoginModel
    //        {
    //            Email = "test@example.com",
    //            Password = "correctpassword"
    //        };

    //        _accountService.Setup(s => s.LoginUser(It.IsAny<LoginModel>()))
    //                           .ReturnsAsync((true, "token123", new List<IdentityError>()));

    //        // Act
    //        var result = await _accountController.Login(loginModel);

    //        // Assert
    //        var okResult = result as OkObjectResult;
    //        Assert.IsNotNull(okResult, "Result should be OkObjectResult.");
    //        Assert.That(okResult.StatusCode, Is.EqualTo(200));

    //        var response = okResult.Value as LoginResponse;
    //        Assert.IsNotNull(response, "Response should not be null.");
    //        Assert.That(response.Token, Is.EqualTo("token123"));

    //    }

    //}
}
