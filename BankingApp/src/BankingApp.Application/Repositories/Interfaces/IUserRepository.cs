namespace BankingApp.Application.Repositories.Interfaces;

using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by their unique identifier.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<User> FindById(int userId);

    /// <summary>Updates an existing user record.</summary>
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdateUser(User user);

    /// <summary>Updates the password hash for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>Gets all active sessions for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Session>> GetActiveSessions(int userId);

    /// <summary>Revokes a single active session owned by the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeSession(int userId, int sessionId);

    /// <summary>Gets all notification preferences for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId);

    /// <summary>Replaces all notification preferences for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences);
}
