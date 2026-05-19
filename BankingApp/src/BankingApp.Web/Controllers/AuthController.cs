namespace BankingApp.Web.Controllers;

using System.Globalization;
using System.Security.Claims;
using BankingApp.Contracts.Features.Authentication.Dtos;
using BankingAppAuthenticationService = BankingApp.Contracts.Features.Authentication.Services.IAuthenticationService;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public sealed class AuthController(BankingAppAuthenticationService authenticationService) : Controller
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
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        model.ReturnUrl = GetSafeReturnUrl(model.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ErrorOr<LoginSuccessResponse> result = await authenticationService.LoginAsync(
            new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            },
            ct);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, result.FirstError.Description);
            return View(model);
        }

        LoginSuccessResponse response = result.Value;
        if (response.Requires2Fa)
        {
            return Redirect($"/Auth/VerifyOtp?userId={response.UserId}");
        }

        if (string.IsNullOrWhiteSpace(response.Token))
        {
            ModelState.AddModelError(string.Empty, "The API did not return an authentication token.");
            return View(model);
        }

        await SignInUserAsync(response.UserId, model.Email, response.Token);
        return Redirect(model.ReturnUrl ?? "/Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await authenticationService.LogoutAsync(ct);
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
            new Claim("userId", userIdValue),
            new Claim("token", token)
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
