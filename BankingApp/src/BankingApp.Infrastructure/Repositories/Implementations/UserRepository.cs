namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using DataAccess.Interfaces;
using ErrorOr;

/// <summary>
///     Provides repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly INotificationPreferenceDataAccess _notificationPreferenceDataAccess;
    private readonly ISessionDataAccess _sessionDataAccess;
    private readonly IUserDataAccess _userDataAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRepository" /> class.
    /// </summary>
    /// <param name="userDataAccess">The user data access component.</param>
    /// <param name="sessionDataAccess">The session data access component.</param>
    /// <param name="notificationPreferenceDataAccess">The notification preference data access component.</param>
    public UserRepository(
        IUserDataAccess userDataAccess,
        ISessionDataAccess sessionDataAccess,
        INotificationPreferenceDataAccess notificationPreferenceDataAccess)
    {
        _userDataAccess = userDataAccess;
        _sessionDataAccess = sessionDataAccess;
        _notificationPreferenceDataAccess = notificationPreferenceDataAccess;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<User> FindById(int userId)
    {
        return _userDataAccess.FindById(userId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="user">The user value.</param>
    public ErrorOr<Success> UpdateUser(User user)
    {
        return _userDataAccess.Update(user);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        return _userDataAccess.UpdatePassword(userId, newPasswordHash);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<List<Session>> GetActiveSessions(int userId)
    {
        return _sessionDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeSession(int userId, int sessionId)
    {
        return _sessionDataAccess.RevokeForUser(userId, sessionId);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId)
    {
        return _notificationPreferenceDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences)
    {
        return _notificationPreferenceDataAccess.Update(userId, preferences);
    }
}
