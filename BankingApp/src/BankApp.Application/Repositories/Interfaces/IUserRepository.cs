// <copyright file="IUserRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IUserRepository interface.
// </summary>

using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by their unique identifier.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<User> FindById(int userId);

    /// <summary>Updates an existing user record.</summary>
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdateUser(User user);

    /// <summary>Updates the password hash for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>Gets all active sessions for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Session>> GetActiveSessions(int userId);

    /// <summary>Revokes a single active session owned by the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> RevokeSession(int userId, int sessionId);

    /// <summary>Gets all OAuth provider links for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<OAuthLink>> GetLinkedProviders(int userId);

    /// <summary>Creates a new OAuth link for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> SaveOAuthLink(int userId, string provider, string providerUserId, string? email);

    /// <summary>Deletes an OAuth link by its identifier.</summary>
    /// <param name="linkId">The linkId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> DeleteOAuthLink(int linkId);

    /// <summary>Gets all notification preferences for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId);

    /// <summary>Replaces all notification preferences for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences);
}