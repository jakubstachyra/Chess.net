using Domain.AuthModels;
using Domain.Users;
using Logic.Tests;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogicTests.AccountServiceTests
{
    //public class LoginUserTests : BaseAccountServiceTest
    //{
    //    [Test]
    //    public async Task LoginUser_ShouldReturnFalse_WhenUserDoesNotExist()
    //    {
    //        var model = new LoginModel
    //        {
    //            Email = "nonexisting@example.com",
    //            Password = "Test1!",
    //        };
    //        _userManager.Setup(um => um.FindByEmailAsync(model.Email))
    //                    .ReturnsAsync((User)null!);

    //        var result = await _accountService.LoginUser(model);

    //        Assert.IsFalse(result.Success);
    //        Assert.IsNull(result.Token);
    //        Assert.IsNotEmpty(result.Errors);
    //        Assert.That(result.Errors.First().Code, Is.EqualTo("UserNotFound"));
    //    }

    //    [Test]
    //    public async Task LoginUser_ShouldReturnFalse_WhenPasswordIsInvalid()
    //    {
    //        var user = new User { Email = "user@example.com" };
    //        var loginModel = new LoginModel { Email = user.Email, Password = "wrongpassword" };

    //        _userManager.Setup(um => um.FindByEmailAsync(loginModel.Email))
    //                    .ReturnsAsync(user);

    //        _signInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, loginModel.Password, false))
    //                      .ReturnsAsync(SignInResult.Failed);

    //        var result = await _accountService.LoginUser(loginModel);

    //        Assert.IsFalse(result.Success);
    //        Assert.IsNull(result.Token);
    //        Assert.IsNotEmpty(result.Errors);
    //        Assert.That(result.Errors.First().Code, Is.EqualTo("InvalidPassword"));
    //    }

    //    [Test]
    //    public async Task LoginUser_ShouldReturnToken_WhenCredentialsAreCorrect()
    //    {
    //        var user = new User { Email = "user@example.com", UserName = "testuser" };
    //        var model = new LoginModel { Email = user.Email, Password = "correctpassword" };

    //        _userManager.Setup(um => um.FindByEmailAsync(model.Email))
    //                    .ReturnsAsync(user);

    //        _signInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, model.Password, false))
    //                      .ReturnsAsync(SignInResult.Success);

    //        _userManager.Setup(um => um.GetRolesAsync(user))
    //                    .ReturnsAsync(new List<string> { "User" });

    //        var result = await _accountService.LoginUser(model);

    //        Assert.IsTrue(result.Success);
    //        Assert.IsNotNull(result.Token);
    //        Assert.IsEmpty(result.Errors);
    //    }
    //}
}
