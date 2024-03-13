using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PROJETOESA.Controllers;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity;
using PROJETOESA.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using Microsoft.Extensions.Options;

namespace AeroHelperTest
{
    public class AccountTest : IClassFixture<AeroHelperContextFixture>
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ICodeGeneratorService> _codeGeneratorServiceMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly CustomAccountController _accountController;
        private readonly AeroHelperContextFixture _fixture;

        public AccountTest(AeroHelperContextFixture fixture)
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<ApplicationUser>>(),
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            _codeGeneratorServiceMock = new Mock<ICodeGeneratorService>();
            _emailSenderMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CustomAccountController>>();
            _fixture = fixture;

            _accountController = new CustomAccountController(
                _userManagerMock.Object,
                _emailSenderMock.Object,
                loggerMock.Object,
                _codeGeneratorServiceMock.Object,
                _fixture.DbContext
                );
        }

        [Fact]
        public async Task SendConfirmationCode_ReturnsOkResult_WhenEmailExists()
        {
            // Arrange
            var model = new CustomRecoverModel { Email = "test@example.com" };
            var user = new ApplicationUser { Email = model.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _codeGeneratorServiceMock.Setup(x => x.GenerateCode()).Returns("123456");

            // Act
            var result = await _accountController.SendConfirmationCode(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Se o e-mail existir, um e-mail de confirmação será enviado.", okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task SendConfirmationCode_ReturnsOkResult_WhenEmailDoesNotExist()
        {
            // Arrange
            var model = new CustomRecoverModel { Email = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _accountController.SendConfirmationCode(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Se o e-mail existir, um e-mail de confirmação será enviado.", okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task SendRecoveryCode_ReturnsOkResult_WhenEmailExists()
        {
            // Arrange
            var model = new CustomRecoverModel { Email = "test@example.com" };
            var user = new ApplicationUser { Email = model.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _codeGeneratorServiceMock.Setup(x => x.GenerateCode()).Returns("123456");

            // Act
            var result = await _accountController.SendRecoveryCode(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Se o e-mail existir, um e-mail de recuperação será enviado.", okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task SendRecoveryCode_ReturnsOkResult_WhenEmailDoesNotExist()
        { 
            // Arrange
            var model = new CustomRecoverModel { Email = "test@example.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _accountController.SendRecoveryCode(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Se o e-mail existir, um e-mail de recuperação será enviado.", okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value));
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsNoContent()
        {
            // Arrange
            var email = "test@example.com";
            var user = new ApplicationUser { Email = email };
            _userManagerMock.Setup(m => m.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.DeleteAsync(user))
                             .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountController.DeleteUser(email);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var email = "test@example.com";
            _userManagerMock.Setup(m => m.FindByEmailAsync(email))
                             .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _accountController.DeleteUser(email);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_DeleteFails_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var email = "test@example.com";
            var user = new ApplicationUser { Email = email };
            _userManagerMock.Setup(m => m.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            var errors = new List<IdentityError> { new IdentityError { Description = "Error deleting user" } };
            _userManagerMock.Setup(m => m.DeleteAsync(user))
                             .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            // Act
            var result = await _accountController.DeleteUser(email);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<List<IdentityError>>(badRequestResult.Value);
            Assert.Single(model); // Assert that only one error is returned
            Assert.Equal("Error deleting user", model[0].Description); // Assert the error message
        }


    }
}