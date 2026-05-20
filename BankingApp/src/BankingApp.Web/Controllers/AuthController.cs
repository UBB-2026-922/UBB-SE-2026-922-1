namespace BankingApp.Web.Controllers;

using System.Globalization;
using System.Security.Claims;
using ClientAuthenticationService = BankingApp.Application.Features.Authentication.Services.IAuthenticationService;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingApp.Contracts.Http;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public sealed class AuthController(
    ClientAuthenticationService authenticationService) : Controller
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
        if (loginResponse.Requires2Fa)
        {
            return Redirect($"/Auth/VerifyOtp?userId={loginResponse.UserId}");
        }

        if (string.IsNullOrWhiteSpace(loginResponse.Token))
        {
            ModelState.AddModelError(string.Empty, "The API did not return an authentication token.");
            return View(loginViewModel);
        }

        await SignInUserAsync(loginResponse.UserId, loginViewModel.Email, loginResponse.Token);
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

    private async Task SignInUserAsync(int userId, string email, string token)
    {
        string userIdValue = userId.ToString(CultureInfo.InvariantCulture);
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userIdValue),
            new Claim(ClaimTypes.Name, email),
            new Claim(AuthClaimTypes.UserId, userIdValue),
            new Claim(AuthClaimTypes.Token, token)
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
