using BankingApp.Api.Controllers;
using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Registration;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Tests.Controller;

[Trait("Category", "Unit")]
public sealed class AuthControllerTests
{
    private readonly Mock<ILoginService> _loginService = MockFactory.CreateLoginService();

    private readonly Mock<IPasswordRecoveryService> _passwordRecoveryService =
        MockFactory.CreatePasswordRecoveryService();

    private readonly Mock<IRegistrationService> _registrationService = MockFactory.CreateRegistrationService();

    [Fact]
    public void Login_WhenSuccessWithFullLogin_ShouldReturnOkWithToken()
    {
        // Arrange
        const int validUserId = 1;
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        _loginService
            .Setup(login => login.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(validUserId, "jwt-token"));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        OkObjectResult? ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Login_WhenRequires2FA_ShouldReturnOk()
    {
        // Arrange
        const int validUserId = 1;
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        _loginService
            .Setup(login => login.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new RequiresTwoFactor(validUserId));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Login_WhenInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };
        _loginService
            .Setup(login => login.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Unauthorized("invalid_credentials", "Invalid credentials."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        UnauthorizedObjectResult? unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Login_WhenAccountLocked_ShouldReturnForbidden()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        _loginService
            .Setup(login => login.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Forbidden("account_locked", "Account is locked."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Register_WhenSuccess_ShouldReturnNoContent()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Password = "Pass123!", FullName = "Test User" };
        _registrationService.Setup(register => register.Register(request)).Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Register_WhenConflict_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest { Email = "dup@test.com", Password = "Pass123!", FullName = "Test" };
        _registrationService
            .Setup(register => register.Register(request))
            .Returns(Error.Conflict("email_registered", "Email already registered."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void Register_WhenServiceFails_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Password = "Pass123!", FullName = "Test" };
        _registrationService
            .Setup(register => register.Register(request))
            .Returns(Error.Failure("database_error", "Service unavailable."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(500);
    }

    [Fact]
    public void VerifyOTP_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        const int validUserId = 1;
        var request = new VerifyOtpRequest { UserId = validUserId, OtpCode = "123456" };
        _loginService
            .Setup(verifiesOtp => verifiesOtp.VerifyOtp(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(validUserId, "jwt-token"));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.VerifyOtp(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyOtp_WhenInvalidOtp_ShouldReturnUnauthorized()
    {
        // Arrange
        const int validUserId = 1;
        var request = new VerifyOtpRequest { UserId = validUserId, OtpCode = "000000" };
        _loginService
            .Setup(verifiesOtp => verifiesOtp.VerifyOtp(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Unauthorized("invalid_otp", "Invalid OTP."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.VerifyOtp(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailProvided_ShouldReturnOk()
    {
        // Arrange
        _passwordRecoveryService
            .Setup(requestsPasswordReset => requestsPasswordReset.RequestPasswordReset("user@test.com"))
            .Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ForgotPassword(new ForgotPasswordRequest { Email = "user@test.com" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ForgotPassword(new ForgotPasswordRequest { Email = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenSuccess_ShouldReturnNoContent()
    {
        // Arrange
        _passwordRecoveryService
            .Setup(resetsPassword => resetsPassword.ResetPassword("valid-token", "NewPass123!"))
            .Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ResetPassword(
            new ResetPasswordRequest
                { Token = "valid-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ResetPassword_WhenTokenMissing_ShouldReturnBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ResetPassword(
            new ResetPasswordRequest
                { Token = string.Empty, NewPassword = "Pass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ResetPassword(
            new ResetPasswordRequest
                { Token = "token", NewPassword = "weak" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenServiceFails_ShouldReturnMappedError()
    {
        // Arrange
        _passwordRecoveryService
            .Setup(resetsPassword => resetsPassword.ResetPassword("bad-token", "NewPass123!"))
            .Returns(Error.Validation("token_expired", "Token has expired."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ResetPassword(
            new ResetPasswordRequest
                { Token = "bad-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Logout_WhenValidToken_ShouldReturnNoContent()
    {
        // Arrange
        _loginService.Setup(logout => logout.Logout("jwt-token")).Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Logout("Bearer jwt-token");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Logout_WhenNoAuthorizationHeader_ShouldReturnBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Logout(string.Empty);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResendOTP_ShouldAlwaysReturnOk()
    {
        // Arrange
        const int validUserId = 1;
        _loginService.Setup(resendOtp => resendOtp.ResendOtp(validUserId, "email")).Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ResendOtp(validUserId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenValid_ShouldReturnNoContent()
    {
        // Arrange
        _passwordRecoveryService
            .Setup(verifiesResetToken => verifiesResetToken.VerifyResetToken("valid-token"))
            .Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.VerifyResetToken(new VerifyTokenDataTransferObject { Token = "valid-token" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenTokenEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.VerifyResetToken(new VerifyTokenDataTransferObject { Token = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(
            _loginService.Object,
            _registrationService.Object,
            _passwordRecoveryService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}