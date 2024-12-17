using Domain.AuthModels;
using Domain.Users;
using Logic.Services;
using Logic.Services.Interfaces;
using Logic.Tests;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LogicTests.AccountServiceTests
{
    public class RegisterUserTests : BaseAccountServiceTest 
    {
        [Test]
        public async Task RegisterUser_ShouldReturnTrueEmptyErrors_WhenDataIsCorrect()
        {
            var model = new RegisterModel
            {
                Username = "username",
                Email = "example@ne.not",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            };

            _userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password)).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "USER")).ReturnsAsync(IdentityResult.Success);

            var result = await _accountService.RegisterUser(model);

            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Errors);
        }
        // Zle haslo
        [Test]
        public async Task RegisterUser_ShouldReturnFalseWithWrongInvalidPassword_WhenPasswordDoesNotMatchCriteria()
        {
            var model = new RegisterModel
            {
                Username = "username",
                Email = "example@ne.not",
                Password = "Password1",
                ConfirmPassword = "Password1"
            };


            _userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { }));
            _userManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "USER")).ReturnsAsync(IdentityResult.Success);

            var result = await _accountService.RegisterUser(model);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Is.TypeOf<List<IdentityError>>());
        }
        [Test]
        public async Task RegisterUser_ShouldReturnFalse_WhenUserExists()
        {
            var model = new RegisterModel
            {
                Username = "username",
                Email = "example@ne.not",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            };

            var identityError = new IdentityError
            {
                Code = "DuplicateUserName",
                Description = "User with this email already exists."
            };

            _userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var result = await _accountService.RegisterUser(model);

            Assert.That(result.Success, Is.False, "The registration should fail when the user already exists.");
            Assert.That(result.Errors, Is.Not.Empty, "Errors should be returned when registration fails.");
            Assert.That(result.Errors.First().Code, Is.EqualTo("DuplicateUserName"),
                "The error code should indicate a duplicate user.");
            Assert.That(result.Errors.First().Description, Is.EqualTo("User with this email already exists."),
                "The error description should indicate the reason for failure.");
        }

    }
}
