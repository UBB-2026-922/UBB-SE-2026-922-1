// <copyright file="ProfileController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ProfileController class.
// </summary>

using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Services.Profile;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Api.Controllers;

/// <summary>
///     Controller responsible for handling user profile-related operations.
///     All endpoints are accessible under the /api/profile route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    private readonly IProfileService _profileService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileController" /> class.
    /// </summary>
    /// <param name="profileService">The profile service used to handle business logic.</param>
    /// <returns>The result of the operation.</returns>
    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    ///     Retrieves the profile information of the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a <see cref="ProfileInfo" /> on success,
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetProfile()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.GetProfile(userId), info => Ok(info));
    }

    /// <summary>
    ///     Updates the personal information of the currently authenticated user.
    ///     The user ID is always taken from the authentication context, not from the request body.
    /// </summary>
    /// <param name="request">The update profile request containing the new personal information.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     or 400/404 if the update fails.
    /// </returns>
    [HttpPut]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        int userId = GetAuthenticatedUserId();
        request.UserId = userId;
        return ToActionResult(_profileService.UpdatePersonalInfo(request));
    }

    /// <summary>
    ///     Changes the password of the currently authenticated user.
    ///     The user ID is always taken from the authentication context, not from the request body.
    /// </summary>
    /// <param name="request">The change password request containing the old and new passwords.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     or 400/404 if the password change fails.
    /// </returns>
    [HttpPut("password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        int userId = GetAuthenticatedUserId();
        request.UserId = userId;
        return ToActionResult(_profileService.ChangePassword(request));
    }

    /// <summary>
    ///     Retrieves all OAuth provider links associated with the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a list of <see cref="OAuthLinkDataTransferObject" /> on success (may be empty),
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("oauth-links")]
    public IActionResult GetOAuthLinks()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.GetOAuthLinks(userId), links => Ok(links));
    }

    /// <summary>
    ///     Links a supported OAuth provider to the currently authenticated user.
    /// </summary>
    /// <param name="request">The provider link request.</param>
    /// <returns>204 No Content on success, or 400/404/409 if linking fails.</returns>
    [HttpPost("oauth/link")]
    public IActionResult LinkOAuth([FromBody] LinkOAuthRequest request)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.LinkOAuth(userId, request.Provider));
    }

    /// <summary>
    ///     Unlinks a supported OAuth provider from the currently authenticated user.
    /// </summary>
    /// <param name="provider">The provider to unlink.</param>
    /// <returns>204 No Content on success, or 400/404 if unlinking fails.</returns>
    [HttpDelete("oauth/{provider}")]
    public IActionResult UnlinkOAuth(string provider)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.UnlinkOAuth(userId, provider));
    }

    /// <summary>
    ///     Retrieves the notification preferences of the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a list of <see cref="NotificationPreferenceDataTransferObject" /> on success (may be empty),
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("notifications/preferences")]
    public IActionResult GetNotificationPreferences()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.GetNotificationPreferences(userId), preferences => Ok(preferences));
    }

    /// <summary>
    ///     Updates the notification preferences of the currently authenticated user.
    /// </summary>
    /// <param name="preferences">The list of updated notification preferences.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     or 400/404 if the update fails.
    /// </returns>
    [HttpPut("notifications/preferences")]
    public IActionResult UpdateNotificationPreferences(
        [FromBody] List<NotificationPreferenceDataTransferObject> preferences)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.UpdateNotificationPreferences(userId, preferences));
    }

    /// <summary>
    ///     Verifies whether the provided password matches the currently authenticated user's password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>
    ///     200 OK with <see langword="true" /> if the password is correct,
    ///     200 OK with <see langword="false" /> if the password does not match,
    ///     or 404/500 on unexpected errors.
    /// </returns>
    [HttpPost("verify-password")]
    public IActionResult VerifyPassword([FromBody] string password)
    {
        int userId = GetAuthenticatedUserId();
        ErrorOr<bool> result = _profileService.VerifyPassword(userId, password);
        return ToActionResult(result, valid => Ok(valid));
    }

    /// <summary>
    ///     Enables 2FA for the currently authenticated user
    ///     using the specified delivery method.
    /// </summary>
    /// <param name="request">The request containing the desired 2FA method.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     or 400/404 if enabling 2FA fails.
    /// </returns>
    [HttpPut("2fa/enable")]
    public IActionResult Enable2Fa([FromBody] Enable2FaRequest request)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.Enable2Fa(userId, request.Method));
    }

    /// <summary>
    ///     Disables 2FA for the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     204 No Content on success,
    ///     or 400/404 if disabling 2FA fails.
    /// </returns>
    [HttpPut("2fa/disable")]
    public IActionResult Disable2Fa()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.Disable2Fa(userId));
    }

    /// <summary>
    ///     Retrieves all active sessions for the currently authenticated user.
    /// </summary>
    /// <returns>
    ///     200 OK with a list of active sessions on success,
    ///     or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("sessions")]
    public IActionResult GetSessions()
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.GetActiveSessions(userId), sessions => Ok(sessions));
    }

    /// <summary>
    ///     Revokes a specific session for the currently authenticated user.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns>
    ///     204 No Content on success,
    ///     or 404/400 if the revocation fails.
    /// </returns>
    [HttpDelete("sessions/{sessionId}")]
    public IActionResult RevokeSession(int sessionId)
    {
        int userId = GetAuthenticatedUserId();
        return ToActionResult(_profileService.RevokeSession(userId, sessionId));
    }
}