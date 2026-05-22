namespace BankingApp.Web.Tests.Controllers;

using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Features.UserProfile.Dtos;
using BankingApp.Contracts.Features.UserProfile.Services;
using BankingApp.Contracts.Http;
using BankingApp.Web.Controllers;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

public sealed class ProfileControllerTests : IDisposable
{
    private const int CurrentSessionId = 5;

    private readonly Mock<IProfileService> _profileServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IAuthenticationService> _aspNetAuthenticationMock = new(MockBehavior.Strict);
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AuthClaimTypes.SessionId, CurrentSessionId.ToString(CultureInfo.InvariantCulture))],
            "Cookies"));

        Mock<IUrlHelperFactory> urlHelperFactoryMock = new(MockBehavior.Loose);
        urlHelperFactoryMock
            .Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(Mock.Of<IUrlHelper>());

        ServiceCollection services = new();
        services.AddSingleton(_aspNetAuthenticationMock.Object);
        services.AddSingleton(urlHelperFactoryMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        _controller = new ProfileController(_profileServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void Security_WhenGet_ShouldReturnView()
    {
        // Act
        IActionResult result = _controller.Security();

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SecurityViewModel>();
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenInvalidModel_ShouldReturnSecurityView()
    {
        // Arrange
        SecurityViewModel model = new();
        _controller.ModelState.AddModelError(nameof(SecurityViewModel.CurrentPassword), "Current password is required.");

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(ProfileController.Security));
        viewResult.Model.Should().Be(model);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangePassword_WhenServiceSucceeds_ShouldRedirectToSecurity()
    {
        // Arrange
        SecurityViewModel model = new()
        {
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!",
            ConfirmNewPassword = "NewPassword1!"
        };

        _profileServiceMock
            .Setup(service => service.ChangePasswordAsync(
                It.Is<ChangePasswordRequest>(request =>
                    request.CurrentPassword == model.CurrentPassword && request.NewPassword == model.NewPassword),
                CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.ChangePassword(model, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Security));
        _controller.TempData["Success"].Should().Be("Password changed successfully.");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenGet_ShouldCallServiceAndReturnViewWithSessionsAndCurrentSessionIdMarked()
    {
        // Arrange
        DateTime firstSessionCreatedAt = new(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc);
        DateTime secondSessionCreatedAt = new(2026, 4, 20, 14, 30, 0, DateTimeKind.Utc);
        List<SessionDto> sessions =
        [
            new SessionDto { Id = CurrentSessionId, Browser = "Chrome", IpAddress = "127.0.0.1", CreatedAt = firstSessionCreatedAt },
            new SessionDto { Id = 9, Browser = "Firefox", IpAddress = "10.0.0.1", CreatedAt = secondSessionCreatedAt }
        ];

        _profileServiceMock
            .Setup(service => service.GetSessionsAsync(CancellationToken.None))
            .ReturnsAsync(sessions);

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        SessionsViewModel model = viewResult.Model.Should().BeOfType<SessionsViewModel>().Subject;
        model.Sessions.Should().HaveCount(2);
        model.CurrentSessionId.Should().Be(CurrentSessionId);
        _profileServiceMock.Verify(service => service.GetSessionsAsync(CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenServiceReturnsError_ShouldSetErrorBannerAndReturnEmptyViewModel()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.GetSessionsAsync(CancellationToken.None))
            .ReturnsAsync(Error.Failure("sessions.unavailable", "Service unavailable."));

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        SessionsViewModel model = viewResult.Model.Should().BeOfType<SessionsViewModel>().Subject;
        model.Sessions.Should().BeEmpty();
        model.CurrentSessionId.Should().Be(CurrentSessionId);
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.Verify(service => service.GetSessionsAsync(CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokeSession_WhenServiceReturnsError_ShouldSetErrorBannerAndRedirectToSessions()
    {
        // Arrange
        const int otherSessionId = 9;
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None))
            .ReturnsAsync(Error.NotFound("session.not_found", "Session not found."));

        // Act
        IActionResult result = await _controller.RevokeSession(otherSessionId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Sessions));
        ((string?)_controller.TempData["Error"]).Should().NotBeNullOrEmpty();
        _profileServiceMock.Verify(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokeSession_WhenRevokingAnotherSession_ShouldSetSuccessBannerAndRedirectToSessions()
    {
        // Arrange
        const int otherSessionId = 9;
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        // Act
        IActionResult result = await _controller.RevokeSession(otherSessionId, CancellationToken.None);

        // Assert
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(ProfileController.Sessions));
        _controller.TempData["Success"].Should().Be("Session revoked successfully.");
        _profileServiceMock.Verify(service => service.RevokeSessionAsync(otherSessionId, CancellationToken.None), Times.Once);
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Sessions_WhenSessionIdClaimMissing_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([], "Cookies"));

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.Sessions(CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task RevokeSession_WhenSessionIdClaimMissing_ShouldSignOutAndRedirectToLogin()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([], "Cookies"));

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.RevokeSession(9, CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task RevokeSession_WhenRevokingCurrentSession_ShouldSignOutAndRedirectToAuthLogin()
    {
        // Arrange
        _profileServiceMock
            .Setup(service => service.RevokeSessionAsync(CurrentSessionId, CancellationToken.None))
            .ReturnsAsync(Result.Success);

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.RevokeSession(CurrentSessionId, CancellationToken.None);

        // Assert
        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        _profileServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }
}
