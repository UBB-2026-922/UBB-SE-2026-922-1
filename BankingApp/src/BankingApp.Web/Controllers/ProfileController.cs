namespace BankingApp.Web.Controllers;

using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Domain.Common.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Profile;

[Authorize]
public class ProfileController(IProfileService profileService) : Controller
{
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        ErrorOr<List<NotificationPreferenceDto>> result =
            await profileService.GetNotificationPreferencesAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load notification preferences. Please try again.";
            return View(new NotificationsViewModel());
        }

        NotificationsViewModel viewModel = new()
        {
            Preferences = result.Value.ConvertAll(preference => new NotificationPreferenceRowViewModel
            {
                Category = preference.Category,
                CategoryDisplayName = preference.Category.ToDisplayName(),
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold
            })
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notifications(NotificationsViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var preferencesToSave = viewModel.Preferences
            .Select(row => new NotificationPreferenceDto
            {
                Category = row.Category,
                PushEnabled = row.PushEnabled,
                EmailEnabled = row.EmailEnabled,
                SmsEnabled = row.SmsEnabled,
                MinAmountThreshold = row.MinAmountThreshold
            })
            .ToList();

        ErrorOr<Success> saveResult =
            await profileService.UpdateNotificationPreferencesAsync(preferencesToSave, cancellationToken);

        TempData[saveResult.IsError ? "Error" : "Success"] = saveResult.IsError
            ? "Could not save notification preferences. Please try again."
            : "Notification preferences saved successfully.";

        return RedirectToAction(nameof(Notifications));
    }
}
