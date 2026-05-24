namespace BankingApp.Web.Tests.Controllers;

using System.Globalization;
using System.Security.Claims;
using ClientAuthenticationService = BankingApp.Application.Features.Authentication.Services.IAuthenticationService;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.Controllers;
using BankingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

public sealed class AuthControllerTests : IDisposable
{
    private readonly Mock<ClientAuthenticationService> _authenticationServiceMock = new(MockBehavior.Strict);
    private readonly Mock<Microsoft.AspNetCore.Authentication.IAuthenticationService> _aspNetAuthenticationMock = new(MockBehavior.Strict);
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        DefaultHttpContext httpContext = new();
        ServiceCollection services = new();
        services.AddSingleton(_aspNetAuthenticationMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        _controller = new AuthController(_authenticationServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    public void Dispose() => _controller.Dispose();

    [Fact]
    public void Login_WhenAnonymousGet_ShouldReturnViewWithReturnUrl()
    {
        IActionResult result = _controller.Login("/Transfers");

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        LoginViewModel model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
        model.ReturnUrl.Should().Be("/Transfers");
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Login_WhenAuthenticatedGet_ShouldRedirectToDashboard()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "7")], "Cookies"));

        IActionResult result = _controller.Login();

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Dashboard");
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenInvalidPost_ShouldReturnViewWithoutCallingApi()
    {
        _controller.ModelState.AddModelError(nameof(LoginViewModel.Email), "Email is required.");
        LoginViewModel model = new() { Email = string.Empty, Password = string.Empty };

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _authenticationServiceMock.VerifyNoOtherCalls();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiErrorPost_ShouldAddModelErrorAndReturnView()
    {
        const string apiMessage = "Invalid email or password.";
        LoginViewModel model = new() { Email = "user@example.com", Password = "bad-password" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(
                It.Is<LoginRequest>(request => request.Email == model.Email && request.Password == model.Password),
                CancellationToken.None))
            .ReturnsAsync(Error.Validation("auth.invalid", apiMessage));

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors.Should().ContainSingle(error => error.ErrorMessage == apiMessage);
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenApiResponseMissingSessionIdPost_ShouldAddErrorAndReturnView()
    {
        LoginViewModel model = new() { Email = "user@example.com", Password = "ValidPassword1!" };

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), CancellationToken.None))
            .ReturnsAsync(new LoginSuccessResponse
            {
                UserId = 15,
                Token = "jwt-token",
                SessionId = null
            });

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);
        _controller.ModelState[string.Empty]!.Errors
            .Should().ContainSingle(error => error.ErrorMessage == "The API did not return a session identifier.");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WhenSuccessfulPost_ShouldSignInAndRedirectToReturnUrl()
    {
        LoginViewModel model = new()
        {
            Email = "user@example.com",
            Password = "ValidPassword1!",
            ReturnUrl = "/Transfers"
        };
        int userId = 15;
        string token = "jwt-token";
        int sessionId = 3;

        _authenticationServiceMock
            .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>(), CancellationToken.None))
            .ReturnsAsync(new LoginSuccessResponse
            {
                UserId = userId,
                Token = token,
                SessionId = sessionId
            });

        _aspNetAuthenticationMock
            .Setup(service => service.SignInAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.Is<ClaimsPrincipal>(principal =>
                    principal.FindFirstValue(ClaimTypes.NameIdentifier) == userId.ToString("D", CultureInfo.InvariantCulture)
                    && principal.FindFirstValue(AuthClaimTypes.UserId) == userId.ToString("D", CultureInfo.InvariantCulture)
                    && principal.FindFirstValue(AuthClaimTypes.Token) == token
                    && principal.FindFirstValue(AuthClaimTypes.SessionId) == sessionId.ToString(CultureInfo.InvariantCulture)
                    && principal.Identity!.Name == model.Email),
                It.Is<AuthenticationProperties>(properties =>
                    properties.IsPersistent
                    && properties.ExpiresUtc.HasValue)))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Login(model, CancellationToken.None);

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Transfers");
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }

    [Fact]
    public async Task Logout_WhenSendingPost_ShouldCallApiSignOutCookieAndRedirectToLogin()
    {
        _authenticationServiceMock
            .Setup(service => service.LogoutAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success);

        _aspNetAuthenticationMock
            .Setup(service => service.SignOutAsync(
                _controller.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null))
            .Returns(Task.CompletedTask);

        IActionResult result = await _controller.Logout(CancellationToken.None);

        RedirectResult redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/Auth/Login");
        
        // Assert that TempData is cleared
        _controller.TempData.Should().BeEmpty();
        
        _authenticationServiceMock.VerifyAll();
        _aspNetAuthenticationMock.VerifyAll();
    }
}
