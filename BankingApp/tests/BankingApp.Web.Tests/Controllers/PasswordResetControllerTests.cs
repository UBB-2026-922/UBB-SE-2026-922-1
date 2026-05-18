namespace BankingApp.Web.Tests.Controllers;

using BankingApp.Application.Features.PasswordReset.Commands;
using BankingApp.Application.Features.PasswordReset.Queries;
using BankingApp.Web.Controllers;
using BankingApp.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class PasswordResetControllerTests : IDisposable
{
    private readonly Mock<ISender> _senderMock = new(MockBehavior.Strict);
    private readonly PasswordResetController _controller;

    public PasswordResetControllerTests()
    {
        _controller = new PasswordResetController(_senderMock.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void ForgotPassword_Get_ShouldReturnViewWithEmptyForgotPasswordViewModel()
    {
        // Act
        IActionResult result = _controller.ForgotPassword();

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        ForgotPasswordViewModel model = viewResult.Model.Should().BeOfType<ForgotPasswordViewModel>().Subject;
        model.Email.Should().BeEmpty();
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_Post_WhenModelStateIsInvalid_ShouldReturnViewWithModelAndNotSendCommand()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(ForgotPasswordViewModel.Email), "Email address is required.");
        ForgotPasswordViewModel model = new() { Email = string.Empty };

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_Post_WhenModelIsValid_ShouldSendCommandWithMatchingEmailAndRedirectWithInfoBanner()
    {
        // Arrange
        const string submittedEmail = "user@example.com";
        ForgotPasswordViewModel model = new() { Email = submittedEmail };

        _senderMock
            .Setup(sender => sender.Send(
                It.Is<ForgotPasswordCommand>(command => command.Email == submittedEmail),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(PasswordResetController.ForgotPassword));
        _controller.TempData["Info"].Should().Be("If that email exists you will receive a password reset link.");

        _senderMock.Verify(sender => sender.Send(It.IsAny<ForgotPasswordCommand>(), CancellationToken.None), Times.Once);
        _senderMock.VerifyNoOtherCalls();
    }

    // Ensures the controller never reveals whether an email is registered (anti-enumeration).
    [Fact]
    public async Task ForgotPassword_Post_WhenSenderReturnsError_ShouldStillRedirectWithInfoBanner()
    {
        // Arrange
        ForgotPasswordViewModel model = new() { Email = "unknown@example.com" };

        _senderMock
            .Setup(sender => sender.Send(It.IsAny<ForgotPasswordCommand>(), CancellationToken.None))
            .ReturnsAsync(Error.NotFound());

        // Act
        IActionResult result = await _controller.ForgotPassword(model, CancellationToken.None);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((string?)_controller.TempData["Info"]).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetPassword_Get_WhenTokenIsNull_ShouldReturnInvalidTokenViewWithoutCallingVerify()
    {
        // Act
        IActionResult result = await _controller.ResetPassword((string?)null, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Get_WhenTokenIsWhitespace_ShouldReturnInvalidTokenViewWithoutCallingVerify()
    {
        // Act
        IActionResult result = await _controller.ResetPassword("   ", CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Get_WhenTokenVerificationFails_ShouldReturnInvalidTokenView()
    {
        // Arrange
        const string expiredToken = "expired-reset-token";

        _senderMock
            .Setup(sender => sender.Send(
                It.Is<VerifyResetTokenQuery>(query => query.Token == expiredToken),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("invalid_token", "Token is invalid or expired."));

        // Act
        IActionResult result = await _controller.ResetPassword(expiredToken, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("InvalidToken");

        _senderMock.Verify(sender => sender.Send(It.IsAny<VerifyResetTokenQuery>(), CancellationToken.None), Times.Once);
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Get_WhenTokenIsValid_ShouldReturnViewWithTokenPrePopulatedInModel()
    {
        // Arrange
        const string validToken = "valid-reset-token";

        _senderMock
            .Setup(sender => sender.Send(
                It.Is<VerifyResetTokenQuery>(query => query.Token == validToken),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ResetPassword(validToken, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull();
        ResetPasswordViewModel model = viewResult.Model.Should().BeOfType<ResetPasswordViewModel>().Subject;
        model.Token.Should().Be(validToken);

        _senderMock.Verify(sender => sender.Send(It.IsAny<VerifyResetTokenQuery>(), CancellationToken.None), Times.Once);
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Post_WhenModelStateIsInvalid_ShouldReturnViewWithModelAndNotSendCommand()
    {
        // Arrange
        _controller.ModelState.AddModelError(nameof(ResetPasswordViewModel.NewPassword), "New password is required.");
        ResetPasswordViewModel model = new() { Token = "some-token", NewPassword = string.Empty, ConfirmPassword = string.Empty };

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Post_WhenCommandFails_ShouldAddErrorDescriptionToModelStateAndReturnView()
    {
        // Arrange
        const string errorDescription = "Token is no longer valid.";
        ResetPasswordViewModel model = new()
        {
            Token = "used-token",
            NewPassword = "ValidPassword1!",
            ConfirmPassword = "ValidPassword1!"
        };

        _senderMock
            .Setup(sender => sender.Send(It.IsAny<ResetPasswordCommand>(), CancellationToken.None))
            .ReturnsAsync(Error.Validation("token_invalid", errorDescription));

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState.IsValid.Should().BeFalse();
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == errorDescription);

        _senderMock.Verify(sender => sender.Send(It.IsAny<ResetPasswordCommand>(), CancellationToken.None), Times.Once);
        _senderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPassword_Post_WhenCommandSucceeds_ShouldSetSuccessBannerAndRedirectToAuthLogin()
    {
        // Arrange
        ResetPasswordViewModel model = new()
        {
            Token = "valid-token",
            NewPassword = "ValidPassword1!",
            ConfirmPassword = "ValidPassword1!"
        };

        _senderMock
            .Setup(sender => sender.Send(
                It.Is<ResetPasswordCommand>(command =>
                    command.Token == model.Token && command.NewPassword == model.NewPassword),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ResetPassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Login");
        redirect.ControllerName.Should().Be("Auth");
        _controller.TempData["Success"].Should().Be("Password reset successfully. Please sign in.");

        _senderMock.Verify(sender => sender.Send(It.IsAny<ResetPasswordCommand>(), CancellationToken.None), Times.Once);
        _senderMock.VerifyNoOtherCalls();
    }
}
