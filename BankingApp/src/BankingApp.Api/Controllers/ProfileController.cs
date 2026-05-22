namespace BankingApp.Api.Controllers;

using Application.Features.UserProfile.Commands;
using Application.Features.UserProfile.Queries;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Exposes profile, password, notification, and session management endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route(ApiEndpoints.Profile.Base)]
public class ProfileController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetProfileQuery(userId), cancellationToken), Ok);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        var command = new UpdateProfileCommand(
            userId,
            request.FullName,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Address,
            request.Nationality,
            request.PreferredLanguage);
        return ToActionResult(await Sender.Send(command, cancellationToken));
    }

    [HttpPut(ApiEndpoints.Profile.ChangePassword)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), cancellationToken));
    }

    [HttpGet(ApiEndpoints.Profile.NotificationPreferences)]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetNotificationPreferencesQuery(userId), cancellationToken), Ok);
    }

    [HttpPut(ApiEndpoints.Profile.NotificationPreferences)]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [FromBody] List<NotificationPreferenceDto> preferences,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new UpdateNotificationPreferencesCommand(userId, preferences), cancellationToken));
    }

    [HttpPost(ApiEndpoints.Profile.VerifyPassword)]
    public async Task<IActionResult> VerifyPassword([FromBody] string password, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new VerifyPasswordQuery(userId, password), cancellationToken),
            isValid => Ok(isValid));
    }

    [HttpGet(ApiEndpoints.Profile.Sessions)]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetActiveSessionsQuery(userId), cancellationToken), Ok);
    }

    [HttpDelete(ApiEndpoints.Profile.SessionById)]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new RevokeSessionCommand(userId, sessionId), cancellationToken));
    }
}
