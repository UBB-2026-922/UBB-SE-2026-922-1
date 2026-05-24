namespace BankingApp.Web.Controllers;

using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Features.UserRegistration.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.Models;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientAuthenticationService = BankingApp.Application.Features.Authentication.Services.IAuthenticationService;

public sealed class AuthController(ClientAuthenticationService authenticationService) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(GetSafeReturnUrl(returnUrl) ?? "/Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = GetSafeReturnUrl(returnUrl) });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel loginViewModel,
        CancellationToken cancellationToken)
    {
        loginViewModel.ReturnUrl = GetSafeReturnUrl(loginViewModel.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(loginViewModel);
        }

        ErrorOr<LoginSuccessResponse> loginResult = await authenticationService.LoginAsync(
            new LoginRequest
            {
                Email = loginViewModel.Email,
                Password = loginViewModel.Password
            },
            cancellationToken);

        if (loginResult.IsError)
        {
            ModelState.AddModelError(string.Empty, loginResult.FirstError.Description);
            return View(loginViewModel);
        }

        LoginSuccessResponse loginResponse = loginResult.Value;

        if (loginResponse.SessionId is null)
        {
            ModelState.AddModelError(string.Empty, "The API did not return a session identifier.");
            return View(loginViewModel);
        }

        await SignInUserAsync(loginResponse.UserId, loginViewModel.Email, loginResponse.Token ?? string.Empty, loginResponse.SessionId.Value);
        return Redirect(loginViewModel.ReturnUrl ?? "/Dashboard");
    }



    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authenticationService.LogoutAsync(cancellationToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect("/Auth/Login");
    }

    private async Task SignInUserAsync(int userId, string email, string token, int sessionId)
    {
        string userIdValue = userId.ToString("D", CultureInfo.InvariantCulture);
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userIdValue),
            new Claim(ClaimTypes.Name, email),
            new Claim(AuthClaimTypes.UserId, userIdValue),
            new Claim(AuthClaimTypes.Token, token),
            new Claim(AuthClaimTypes.SessionId, sessionId.ToString(CultureInfo.InvariantCulture))
        ];

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties properties = new()
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterModel registerModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(registerModel);
        }

        ErrorOr<Success> registerResult = await authenticationService.RegisterAsync(
            new RegisterRequest
            {
                Email = registerModel.Email,
                Password = registerModel.Password,
                FullName = registerModel.FullName
            },
            cancellationToken);

        if (registerResult.IsError)
        {
            string errorMessage = MapRegisterError(registerResult.FirstError);
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(registerModel);
        }

        TempData["Success"] = "Account created! You can now sign in.";
        return Redirect("/Auth/Login");
    }

    private static string MapRegisterError(Error error)
    {
        if (error.Type == ErrorType.Conflict)
        {
            return "This email is already registered.";
        }

        if (error.Code == "invalid_email")
        {
            return "Please enter a valid email address.";
        }

        if (error.Code == "weak_password")
        {
            return "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.";
        }

        return "Something went wrong. Please try again.";
    }

    private static string? GetSafeReturnUrl(string? returnUrl)
    {
        return IsLocalReturnUrl(returnUrl) ? returnUrl : null;
    }

    private static bool IsLocalReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl)
               && returnUrl[0] == '/'
               && (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'));
    }
}