// <copyright file="UserRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the UserRepository class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Provides repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly INotificationPreferenceDataAccess _notificationPreferenceDataAccess;
    private readonly IOAuthLinkDataAccess _oauthLinkDataAccess;
    private readonly ISessionDataAccess _sessionDataAccess;
    private readonly IUserDataAccess _userDataAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRepository" /> class.
    /// </summary>
    /// <param name="userDataAccess">The user data access component.</param>
    /// <param name="sessionDataAccess">The session data access component.</param>
    /// <param name="oauthLinkDataAccess">The OAuth link data access component.</param>
    /// <param name="notificationPreferenceDataAccess">The notification preference data access component.</param>
    public UserRepository(
        IUserDataAccess userDataAccess,
        ISessionDataAccess sessionDataAccess,
        IOAuthLinkDataAccess oauthLinkDataAccess,
        INotificationPreferenceDataAccess notificationPreferenceDataAccess)
    {
        _userDataAccess = userDataAccess;
        _sessionDataAccess = sessionDataAccess;
        _notificationPreferenceDataAccess = notificationPreferenceDataAccess;
        _oauthLinkDataAccess = oauthLinkDataAccess;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="id">The id value.</param>
    public ErrorOr<User> FindById(int id)
    {
        return _userDataAccess.FindById(id);
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
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<List<OAuthLink>> GetLinkedProviders(int userId)
    {
        return _oauthLinkDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return _oauthLinkDataAccess.Create(userId, provider, providerUserId, email);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="linkId">The linkId value.</param>
    public ErrorOr<Success> DeleteOAuthLink(int linkId)
    {
        return _oauthLinkDataAccess.Delete(linkId);
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