namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Features.UserProfile.Dtos;
using BankingApp.Contracts.Features.UserProfile.Services;
using BankingApp.Contracts.Http;
using BankingApp.Web.ViewModels;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProfileController(IProfileService profileService) : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(Sessions));

    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        int currentSessionId = ParseCurrentSessionId();

        ErrorOr<List<SessionDto>> result = await profileService.GetSessionsAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load sessions.";
            return View(new SessionsViewModel { Sessions = [], CurrentSessionId = currentSessionId });
        }

        return View(new SessionsViewModel { Sessions = result.Value, CurrentSessionId = currentSessionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Profile/RevokeSession/{sessionId:int}")]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int currentSessionId = ParseCurrentSessionId();

        ErrorOr<Success> result = await profileService.RevokeSessionAsync(sessionId, cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not revoke that session.";
            return RedirectToAction(nameof(Sessions));
        }

        if (sessionId == currentSessionId)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        TempData["Success"] = "Session revoked successfully.";
        return RedirectToAction(nameof(Sessions));
    }

    private int ParseCurrentSessionId()
    {
        string? claim = User.FindFirst(AuthClaimTypes.SessionId)?.Value;
        return int.TryParse(claim, out int sessionId) ? sessionId : 0;
    }
}
