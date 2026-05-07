namespace BankingApp.Desktop.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Profile;
using ErrorOr;

/// <summary>
///     Defines the desktop client boundary for profile, security, notification,
///     and session-management operations.
/// </summary>
public interface IProfileClientService
{
    /// <summary>
    ///     Loads the current user's profile.
    /// </summary>
    public Task<ErrorOr<ProfileDto>> GetProfileAsync();

    /// <summary>
    ///     Persists profile changes for the current user.
    /// </summary>
    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request);

    /// <summary>
    ///     Verifies the current password against the server.
    /// </summary>
    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password);

    /// <summary>
    ///     Changes the current user's password.
    /// </summary>
    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request);

    /// <summary>
    ///     Enables two-factor authentication for the current user.
    /// </summary>
    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request);

    /// <summary>
    ///     Disables two-factor authentication for the current user.
    /// </summary>
    public Task<ErrorOr<Success>> Disable2FaAsync();

    /// <summary>
    ///     Loads the current user's notification preferences.
    /// </summary>
    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync();

    /// <summary>
    ///     Persists notification preferences for the current user.
    /// </summary>
    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences);

    /// <summary>
    ///     Loads the current user's active sessions.
    /// </summary>
    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync();

    /// <summary>
    ///     Revokes a specific active session.
    /// </summary>
    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId);
}
