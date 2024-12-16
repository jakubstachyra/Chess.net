using Domain.AuthModels;
using Logic.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTests.AccountServiceTests
{
    [TestFixture]
    public class LoginUserTests : BaseAccountServiceTest
    {
        [Test]
        public async LoginUser_ShouldReturnTrue_WhenCredentialsAreCorrect()
        {
            //Arrange
            var model = new LoginModel
            {
                Email = "test@example.com",
                Password = "Test1!",
            };
            //Act

            //Assert
        }
    }
}
