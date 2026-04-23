// <copyright file="AuthControllerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Api.Controllers;
using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Registration;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Tests.Controller;

/// <summary>
///     Unit tests for <see cref="AuthController" /> verifying route contracts,
///     status codes, and public endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class AuthControllerTests
{
    private readonly Mock<ILoginService> _loginService = MockFactory.CreateLoginService();

    private readonly Mock<IPasswordRecoveryService> _passwordRecoveryService =
        MockFactory.CreatePasswordRecoveryService();

    private readonly Mock<IRegistrationService> _registrationService = MockFactory.CreateRegistrationService();

    /// <summary>
    ///     Verifies the Login_WhenSuccessWithFullLogin_ReturnsOkWithToken scenario.
    /// </summary>
    [Fact]
    public void Login_WhenSuccessWithFullLogin_ReturnsOkWithToken()
    {
        // Arrange
        var validUserId = 1;
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

    /// <summary>
    ///     Verifies the Login_WhenRequires2FA_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void Login_WhenRequires2FA_ReturnsOk()
    {
        // Arrange
        var validUserId = 1;
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

    /// <summary>
    ///     Verifies the Login_WhenInvalidCredentials_ReturnsUnauthorized scenario.
    /// </summary>
    [Fact]
    public void Login_WhenInvalidCredentials_ReturnsUnauthorized()
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

    /// <summary>
    ///     Verifies the Login_WhenAccountLocked_ReturnsForbidden scenario.
    /// </summary>
    [Fact]
    public void Login_WhenAccountLocked_ReturnsForbidden()
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

    /// <summary>
    ///     Verifies the Login_WhenUnexpectedSuccessType_ReturnsInternalServerError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenUnexpectedSuccessType_ReturnsInternalServerError()
    {
        // Arrange
        var validUserId = 1;
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        _loginService
            .Setup(login => login.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new UnexpectedLoginSuccess(validUserId));
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(500);
    }

    /// <summary>
    ///     Verifies the Register_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void Register_WhenSuccess_ReturnsNoContent()
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

    /// <summary>
    ///     Verifies the Register_WhenConflict_ReturnsConflict scenario.
    /// </summary>
    [Fact]
    public void Register_WhenConflict_ReturnsConflict()
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

    /// <summary>
    ///     Verifies the Register_WhenServiceFails_ReturnsInternalServerError scenario.
    /// </summary>
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

    /// <summary>
    ///     Verifies the VerifyOTP_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenSuccess_ReturnsOk()
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

    /// <summary>
    ///     Verifies the VerifyOTP_WhenInvalidOTP_ReturnsUnauthorized scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenInvalidOTP_ReturnsUnauthorized()
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

    /// <summary>
    ///     Verifies the ForgotPassword_WhenEmailProvided_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void ForgotPassword_WhenEmailProvided_ReturnsOk()
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

    /// <summary>
    ///     Verifies the ForgotPassword_WhenEmailEmpty_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public void ForgotPassword_WhenEmailEmpty_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.ForgotPassword(new ForgotPasswordRequest { Email = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenSuccess_ReturnsNoContent()
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

    /// <summary>
    ///     Verifies the ResetPassword_WhenTokenMissing_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenMissing_ReturnsBadRequest()
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

    /// <summary>
    ///     Verifies the ResetPassword_WhenWeakPassword_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenWeakPassword_ReturnsBadRequest()
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

    /// <summary>
    ///     Verifies the ResetPassword_WhenServiceFails_ReturnsMappedError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenServiceFails_ReturnsMappedError()
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

    /// <summary>
    ///     Verifies the Logout_WhenValidToken_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void Logout_WhenValidToken_ReturnsNoContent()
    {
        // Arrange
        _loginService.Setup(logout => logout.Logout("jwt-token")).Returns(Result.Success);
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Logout("Bearer jwt-token");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    ///     Verifies the Logout_WhenNoAuthorizationHeader_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public void Logout_WhenNoAuthorizationHeader_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.Logout(string.Empty);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    ///     Verifies the ResendOTP_AlwaysReturnsOk scenario.
    /// </summary>
    [Fact]
    public void ResendOTP_AlwaysReturnsOk()
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

    /// <summary>
    ///     Verifies the VerifyResetToken_WhenValid_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenValid_ReturnsNoContent()
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

    /// <summary>
    ///     Verifies the VerifyResetToken_WhenTokenEmpty_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenTokenEmpty_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = CreateController();

        // Act
        IActionResult result = controller.VerifyResetToken(new VerifyTokenDataTransferObject { Token = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    ///     Verifies the OAuthLogin_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLogin_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var validUserId = 1;
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        _loginService
            .Setup(oauthLoginAsync => oauthLoginAsync.OAuthLoginAsync(request, It.IsAny<SessionMetadata?>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(validUserId, "jwt-token"));
        AuthController controller = CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    ///     Verifies the OAuthLogin_WhenProviderMissing_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLogin_WhenProviderMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = string.Empty, ProviderToken = "token" };
        AuthController controller = CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    ///     Verifies the OAuthLogin_WhenProviderTokenMissing_ReturnsBadRequest scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLogin_WhenProviderTokenMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = string.Empty };
        AuthController controller = CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    ///     Verifies the OAuthLogin_WhenAccountLocked_ReturnsForbidden scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLogin_WhenAccountLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        _loginService
            .Setup(oauthLoginAsync => oauthLoginAsync.OAuthLoginAsync(request, It.IsAny<SessionMetadata?>()))
            .ReturnsAsync(Error.Forbidden("account_locked", "Account is locked."));
        AuthController controller = CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(
            _loginService.Object,
            _registrationService.Object,
            _passwordRecoveryService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    private sealed class UnexpectedLoginSuccess : LoginSuccess
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnexpectedLoginSuccess" /> class.
        /// </summary>
        public UnexpectedLoginSuccess(int userId)
            : base(userId)
        {
        }
    }
}