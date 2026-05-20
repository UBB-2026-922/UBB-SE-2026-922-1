namespace BankingApp.Web.Tests.Controllers;

using Application.Features.Authentication.Services;
using Contracts.Features.PasswordReset.Dtos;
using Web.Controllers;
using ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class PasswordResetControllerTests : IDisposable
{
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new(MockBehavior.Strict);
    private readonly PasswordResetController _controller;

    public PasswordResetControllerTests()
    {
        _controller = new PasswordResetController(_authenticationServiceMock.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void ForgotPassword_WhenSendingGet_ShouldReturnViewWithEmptyForgotPasswordViewModel()
    {
        // Act
        IActionResult result = _controller.ForgotPassword();

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ForgotPasswordViewModel model = viewResult.Model.Should().BeOfType<ForgotPasswordViewModel>().Subject;
        model.Email.Should().BeEmpty();
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_WhenInvalidPost_ShouldReturnViewWithModelAndNotSendCommand()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(ForgotPasswordViewModel.Email), "Email address is required.");
        ForgotPasswordViewModel model = new() { Email = string.Empty };

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_WhenValidPost_ShouldSendCommandWithMatchingEmailAndRedirectWithInfoBanner()
    {
        // Arrange
        const string submittedEmail = "user@example.com";
        ForgotPasswordViewModel model = new() { Email = submittedEmail };

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.ForgotPasswordAsync(
                It.Is<ForgotPasswordRequest>(request => request.Email == submittedEmail),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PasswordResetController.ForgotPassword));
        _controller.TempData["Info"].Should().Be("If that email exists you will receive a password reset link.");

        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.ForgotPasswordAsync(
                It.IsAny<ForgotPasswordRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    // Ensures the controller never reveals whether an email is registered (anti-enumeration).
    [Fact]
    public async Task ForgotPassword_WhenSenderErrorPost_ShouldStillRedirectWithInfoBanner()
    {
        // Arrange
        ForgotPasswordViewModel model = new() { Email = "unknown@example.com" };

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.ForgotPasswordAsync(
                It.IsAny<ForgotPasswordRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.NotFound());

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((string?)_controller.TempData["Info"]).Should().NotBeNullOrEmpty();
        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.ForgotPasswordAsync(
                It.IsAny<ForgotPasswordRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenTokenNullGet_ShouldReturnInvalidTokenViewWithoutCallingVerify()
    {
        // Act
        IActionResult result = await _controller.ResetPassword((string?)null, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenWhitespaceTokenGet_ShouldReturnInvalidTokenViewWithoutCallingVerify()
    {
        // Act
        IActionResult result = await _controller.ResetPassword("   ", CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenVerificationFailsGet_ShouldReturnInvalidTokenView()
    {
        // Arrange
        const string expiredToken = "expired-reset-token";

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.VerifyResetTokenAsync(
                It.Is<VerifyResetTokenRequest>(request => request.Token == expiredToken),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("invalid_token", "Token is invalid or expired."));

        // Act
        IActionResult result = await _controller.ResetPassword(expiredToken, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");

        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.VerifyResetTokenAsync(
                It.IsAny<VerifyResetTokenRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenValidTokenGet_ShouldReturnViewWithTokenPrePopulatedInModel()
    {
        // Arrange
        const string validToken = "valid-reset-token";

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.VerifyResetTokenAsync(
                It.Is<VerifyResetTokenRequest>(request => request.Token == validToken),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ResetPassword(validToken, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull();
        ResetPasswordViewModel model = viewResult.Model.Should().BeOfType<ResetPasswordViewModel>().Subject;
        model.Token.Should().Be(validToken);

        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.VerifyResetTokenAsync(
                It.IsAny<VerifyResetTokenRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenInvalidPost_ShouldReturnViewWithModelAndNotSendCommand()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(ResetPasswordViewModel.NewPassword), "New password is required.");
        ResetPasswordViewModel model = new() { Token = "some-token", NewPassword = string.Empty, ConfirmPassword = string.Empty };

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenCommandFailsPost_ShouldAddErrorDescriptionToModelStateAndReturnView()
    {
        // Arrange
        const string errorDescription = "Token is no longer valid.";
        ResetPasswordViewModel model = new()
        {
            Token = "used-token",
            NewPassword = "ValidPassword1!",
            ConfirmPassword = "ValidPassword1!"
        };

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.ResetPasswordAsync(
                It.IsAny<ResetPasswordRequest>(),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("token_invalid", errorDescription));

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState.IsValid.Should().BeFalse();
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == errorDescription);

        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.ResetPasswordAsync(
                It.IsAny<ResetPasswordRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_WhenCommandSucceedsPost_ShouldSetSuccessBannerAndRedirectToAuthLogin()
    {
        // Arrange
        ResetPasswordViewModel model = new()
        {
            Token = "valid-token",
            NewPassword = "ValidPassword1!",
            ConfirmPassword = "ValidPassword1!"
        };

        _authenticationServiceMock
            .Setup(authenticationService => authenticationService.ResetPasswordAsync(
                It.Is<ResetPasswordRequest>(request =>
                    request.Token == model.Token && request.NewPassword == model.NewPassword),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Auth");
        _controller.TempData["Success"].Should().Be("Password reset successfully. Please sign in.");

        _authenticationServiceMock.Verify(
            authenticationService => authenticationService.ResetPasswordAsync(
                It.IsAny<ResetPasswordRequest>(),
                CancellationToken.None),
            Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }
}
