namespace BankingApp.Api.Controllers;

using Application.Features.UserProfile.Commands;
using Application.Features.UserProfile.Dtos;
using Application.Features.UserProfile.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Exposes profile, password, notification, two-factor, and session management endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
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

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), cancellationToken));
    }

    [HttpGet("notifications/preferences")]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetNotificationPreferencesQuery(userId), cancellationToken), Ok);
    }

    [HttpPut("notifications/preferences")]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [FromBody] List<NotificationPreferenceDto> preferences,
        CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new UpdateNotificationPreferencesCommand(userId, preferences), cancellationToken));
    }

    [HttpPost("verify-password")]
    public async Task<IActionResult> VerifyPassword([FromBody] string password, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(
            await Sender.Send(new VerifyPasswordQuery(userId, password), cancellationToken),
            isValid => Ok(isValid));
    }

    [HttpPut("2fa/enable")]
    public async Task<IActionResult> Enable2Fa([FromBody] EnableTwoFaRequest request, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new Enable2FaCommand(userId, request.Method), cancellationToken));
    }

    [HttpPut("2fa/disable")]
    public async Task<IActionResult> Disable2Fa(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new Disable2FaCommand(userId), cancellationToken));
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new GetActiveSessionsQuery(userId), cancellationToken), Ok);
    }

    [HttpDelete("sessions/{sessionId:int}")]
    public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken cancellationToken)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(await Sender.Send(new RevokeSessionCommand(userId, sessionId), cancellationToken));
    }
}
