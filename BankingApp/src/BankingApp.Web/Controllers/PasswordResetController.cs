namespace BankingApp.Web.Controllers;

using Application.Features.Authentication.Services;
using BankingApp.Web.ViewModels;
using Contracts.Features.PasswordReset.Dtos;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class PasswordResetController(IAuthenticationService authenticationService) : Controller
{
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await authenticationService.ForgotPasswordAsync(
            new ForgotPasswordRequest { Email = model.Email },
            cancellationToken);

        TempData["Info"] = "If that email exists you will receive a password reset link.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    public async Task<IActionResult> ResetPassword(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("InvalidToken");
        }

        ErrorOr<Success> verification = await authenticationService.VerifyResetTokenAsync(
            new VerifyResetTokenRequest { Token = token },
            cancellationToken);
        if (verification.IsError)
        {
            return View("InvalidToken");
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ErrorOr<Success> result = await authenticationService.ResetPasswordAsync(
            new ResetPasswordRequest
            {
                Token = model.Token,
                NewPassword = model.NewPassword,
            },
            cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, result.FirstError.Description);
            return View(model);
        }

        TempData["Success"] = "Password reset successfully. Please sign in.";
        return RedirectToAction("Login", "Auth");
    }
}
