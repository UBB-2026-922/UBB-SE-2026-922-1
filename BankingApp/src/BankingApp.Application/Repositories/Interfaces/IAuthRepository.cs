// <copyright file="IAuthRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IAuthRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines repository operations for authentication, session management, and account security.
/// </summary>
public interface IAuthRepository
{
    /// <summary>Finds a user by their email address.</summary>
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<User> FindUserByEmail(string email);

    /// <summary>Creates a new user and initializes their default notification preferences.</summary>
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> CreateUser(User user);

    /// <summary>Finds an OAuth link by its provider name and provider-specific user identifier.</summary>
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<OAuthLink> FindOAuthLink(string provider, string providerUserId);

    /// <summary>Creates a new OAuth link record.</summary>
    /// <param name="link">The link value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> CreateOAuthLink(OAuthLink link);

    /// <summary>Creates a new session for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="token">The token value.</param>
    /// <param name="deviceInfo">The deviceInfo value.</param>
    /// <param name="browser">The browser value.</param>
    /// <param name="remoteIpAddress">The remote IP address value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Session> CreateSession(
        int userId,
        string token,
        string? deviceInfo,
        string? browser,
        string? remoteIpAddress);

    /// <summary>Finds an active session by its token.</summary>
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Session> FindSessionByToken(string token);

    /// <summary>Determines whether an active session exists for the specified token.</summary>
    /// <param name="token">The session token to validate.</param>
    /// <returns><see langword="true" /> when the session exists and is active; otherwise an error.</returns>
    ErrorOr<bool> IsSessionActive(string token);

    /// <summary>Persists a password reset token.</summary>
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> SavePasswordResetToken(PasswordResetToken token);

    /// <summary>Finds a password reset token by its hash.</summary>
    /// <param name="tokenHash">The tokenHash value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<PasswordResetToken> FindPasswordResetToken(string tokenHash);

    /// <summary>Marks a password reset token as used.</summary>
    /// <param name="tokenId">The tokenId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> MarkPasswordResetTokenAsUsed(int tokenId);

    /// <summary>Deletes all expired password reset tokens from the system.</summary>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> DeleteExpiredPasswordResetTokens();

    /// <summary>Revokes all active sessions for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> InvalidateAllSessions(int userId);

    /// <summary>Finds a user by their unique identifier.</summary>
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<User> FindUserById(int id);

    /// <summary>Updates the password hash for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>Finds all active sessions for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<List<Session>> FindSessionsByUserId(int userId);

    /// <summary>Revokes the specified session, requiring the service layer to create a new one.</summary>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> UpdateSessionToken(int sessionId);

    /// <summary>Increments the failed login attempt counter for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> IncrementFailedAttempts(int userId);

    /// <summary>Resets the failed login attempt counter to zero for the specified user.</summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> ResetFailedAttempts(int userId);

    /// <summary>Locks the specified user account until the given time.</summary>
    /// <param name="userId">The userId value.</param>
    /// <param name="lockoutEnd">The lockoutEnd value.</param>
    /// <returns>The result of the operation.</returns>
    ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd);
}
