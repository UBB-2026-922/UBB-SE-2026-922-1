namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Http;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels;
using ViewModels.Profile;

[Authorize]
public class ProfileController(IProfileService profileService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<ProfileDto> result = await profileService.GetProfileAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load profile data. Please try again.";
            return View(new ProfileIndexViewModel());
        }

        ProfileDto profile = result.Value;
        ProfileIndexViewModel viewModel = new()
        {
            FullName = profile.FullName ?? string.Empty,
            Email = profile.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            Is2FaEnabled = profile.Is2FaEnabled,
        };

        return View(viewModel);
    }

    public async Task<IActionResult> PersonalInfo(CancellationToken cancellationToken)
    {
        ErrorOr<ProfileDto> result = await profileService.GetProfileAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load personal information.";
            return RedirectToAction(nameof(Index));
        }

        ProfileDto profile = result.Value;
        PersonalInfoViewModel viewModel = new()
        {
            FullName = profile.FullName ?? string.Empty,
            Email = profile.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonalInfo(PersonalInfoViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        UpdateProfileRequest request = new()
        {
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
        };

        ErrorOr<Success> result = await profileService.UpdateProfileAsync(request, cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not update personal information. Please try again.");
            return View(model);
        }

        TempData["Success"] = "Personal information updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
    {
        int? currentSessionId = ParseCurrentSessionId();
        if (currentSessionId is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        ErrorOr<List<SessionDto>> result = await profileService.GetSessionsAsync(cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not load sessions.";
            return View(new SessionsViewModel { Sessions = [], CurrentSessionId = currentSessionId.Value });
        }

        return View(new SessionsViewModel { Sessions = result.Value, CurrentSessionId = currentSessionId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Profile/RevokeSession/{sessionId:int}")]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int? currentSessionId = ParseCurrentSessionId();
        if (currentSessionId is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        ErrorOr<Success> result = await profileService.RevokeSessionAsync(sessionId, cancellationToken);
        if (result.IsError)
        {
            TempData["Error"] = "Could not revoke that session.";
            return RedirectToAction(nameof(Sessions));
        }

        if (sessionId == currentSessionId.Value)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        TempData["Success"] = "Session revoked successfully.";
        return RedirectToAction(nameof(Sessions));
    }

    private int? ParseCurrentSessionId()
    {
        string? claim = User.FindFirst(AuthClaimTypes.SessionId)?.Value;
        return int.TryParse(claim, out int sessionId) ? sessionId : null;
    }
}
