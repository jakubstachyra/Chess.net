using Domain.AuthModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ApiTests.AccountControllerTests
{
    //public class AccountControllerRegisterTests : AccountControllerBaseTests
    //{

    //    [Test]
    //    public async Task Register_ShouldReturnOK_WhenRegistrationIsSuccessful()
    //    {
    //        var registerModel = new RegisterModel
    //        {
    //            Username = "username",
    //            Email = "test@example.com",
    //            Password = "password",
    //            ConfirmPassword = "password"
    //        };

    //        _accountService.Setup(s => s.RegisterUser(registerModel))
    //                           .ReturnsAsync((true, new List<IdentityError>()));

    //        var result = await _accountController.Register(registerModel);

    //        var okResult = result as OkObjectResult;

    //        Assert.IsNotNull(okResult);
    //        Assert.That(okResult.StatusCode, Is.EqualTo(200));
    //    }

    //    [Test]
    //    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    //    {

    //        var registerModel = new RegisterModel {
    //            Username = "username",
    //            Email = "test@example.com",
    //            Password = "Password123!",
    //            ConfirmPassword = "Password123!"
    //        };
    //        var errors = new List<IdentityError>
    //        {
    //            new IdentityError { Code = "DuplicateUser", Description = "User already exists" }
    //        };

    //        _accountService.Setup(s => s.RegisterUser(registerModel))
    //                           .ReturnsAsync((false, errors));

    //        var result = await _accountController.Register(registerModel);

    //        var badRequestResult = result as BadRequestObjectResult;
    //        Assert.IsNotNull(badRequestResult);
    //        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));

    //        var returnedErrors = badRequestResult.Value as IEnumerable<IdentityError>;
    //        Assert.That(returnedErrors, Is.Not.Empty);
    //    }
    //}
}
