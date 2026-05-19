namespace BankingApp.Web.Controllers;

using BankingApp.Application.Features.PasswordReset.Commands;
using BankingApp.Application.Features.PasswordReset.Queries;
using BankingApp.Web.ViewModels;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class PasswordResetController(ISender sender) : Controller
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

        await sender.Send(new ForgotPasswordCommand(model.Email), cancellationToken);

        TempData["Info"] = "If that email exists you will receive a password reset link.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    public async Task<IActionResult> ResetPassword(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("InvalidToken");
        }

        ErrorOr<Success> verification = await sender.Send(new VerifyResetTokenQuery(token), cancellationToken);
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

        ErrorOr<Success> result = await sender.Send(
            new ResetPasswordCommand(model.Token, model.NewPassword),
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
