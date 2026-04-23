// <copyright file="AuthRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AuthRepository class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Extensions;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Provides repository operations for authentication, session management, and account security.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly INotificationPreferenceDataAccess _notificationPreferenceDataAccess;
    private readonly IOAuthLinkDataAccess _oauthLinkDataAccess;
    private readonly IPasswordResetTokenDataAccess _passwordResetTokenDataAccess;
    private readonly ISessionDataAccess _sessionDataAccess;
    private readonly IUserDataAccess _userDataAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthRepository" /> class.
    /// </summary>
    /// <param name="userDataAccess">The user data access component.</param>
    /// <param name="sessionDataAccess">The session data access component.</param>
    /// <param name="oauthLinkDataAccess">The OAuth link data access component.</param>
    /// <param name="passwordResetTokenDataAccess">The password reset token data access component.</param>
    /// <param name="notificationPreferenceDataAccess">The notification preference data access component.</param>
    public AuthRepository(
        IUserDataAccess userDataAccess,
        ISessionDataAccess sessionDataAccess,
        IOAuthLinkDataAccess oauthLinkDataAccess,
        IPasswordResetTokenDataAccess passwordResetTokenDataAccess,
        INotificationPreferenceDataAccess notificationPreferenceDataAccess)
    {
        _userDataAccess = userDataAccess;
        _sessionDataAccess = sessionDataAccess;
        _oauthLinkDataAccess = oauthLinkDataAccess;
        _passwordResetTokenDataAccess = passwordResetTokenDataAccess;
        _notificationPreferenceDataAccess = notificationPreferenceDataAccess;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="email">The email value.</param>
    public ErrorOr<User> FindUserByEmail(string email)
    {
        return _userDataAccess.FindByEmail(email);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="id">The id value.</param>
    public ErrorOr<User> FindUserById(int id)
    {
        return _userDataAccess.FindById(id);
    }

    /// <inheritdoc />
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> CreateUser(User user)
    {
        ErrorOr<Success> createResult = _userDataAccess.Create(user);
        if (createResult.IsError)
        {
            return createResult.FirstError;
        }

        ErrorOr<User> createdUser = _userDataAccess.FindByEmail(user.Email);
        if (createdUser.IsError)
        {
            return createdUser.FirstError;
        }

        foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
        {
            ErrorOr<Success> preferenceResult =
                _notificationPreferenceDataAccess.Create(createdUser.Value.Id, type.ToDisplayName());
            if (preferenceResult.IsError)
            {
                return preferenceResult.FirstError;
            }
        }

        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<OAuthLink> FindOAuthLink(string provider, string providerUserId)
    {
        return _oauthLinkDataAccess.FindByProvider(provider, providerUserId);
    }

    /// <inheritdoc />
    /// <param name="link">The link value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> CreateOAuthLink(OAuthLink link)
    {
        return _oauthLinkDataAccess.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="token">The token value.</param>
    /// <param name="deviceInfo">The deviceInfo value.</param>
    /// <param name="browser">The browser value.</param>
    /// <param name="remoteIpAddress">The remote IP address value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Session> CreateSession(
        int userId,
        string token,
        string? deviceInfo,
        string? browser,
        string? remoteIpAddress)
    {
        return _sessionDataAccess.Create(userId, token, deviceInfo, browser, remoteIpAddress);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="token">The token value.</param>
    public ErrorOr<Session> FindSessionByToken(string token)
    {
        return _sessionDataAccess.FindByToken(token);
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> IsSessionActive(string token)
    {
        return _sessionDataAccess.FindByToken(token).Then(_ => true);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<List<Session>> FindSessionsByUserId(int userId)
    {
        return _sessionDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="sessionId">The sessionId value.</param>
    public ErrorOr<Success> UpdateSessionToken(int sessionId)
    {
        return _sessionDataAccess.Revoke(sessionId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<Success> InvalidateAllSessions(int userId)
    {
        return _sessionDataAccess.RevokeAll(userId);
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> SavePasswordResetToken(PasswordResetToken token)
    {
        return _passwordResetTokenDataAccess.Create(token.UserId, token.TokenHash, token.ExpiresAt)
            .Then(_ => Result.Success);
    }

    /// <inheritdoc />
    /// <param name="tokenHash">The tokenHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<PasswordResetToken> FindPasswordResetToken(string tokenHash)
    {
        return _passwordResetTokenDataAccess.FindByToken(tokenHash);
    }

    /// <inheritdoc />
    /// <param name="tokenId">The tokenId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> MarkPasswordResetTokenAsUsed(int tokenId)
    {
        return _passwordResetTokenDataAccess.MarkAsUsed(tokenId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> DeleteExpiredPasswordResetTokens()
    {
        return _passwordResetTokenDataAccess.DeleteExpired();
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<Success> IncrementFailedAttempts(int userId)
    {
        return _userDataAccess.IncrementFailedAttempts(userId);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    /// <param name="userId">The userId value.</param>
    public ErrorOr<Success> ResetFailedAttempts(int userId)
    {
        return _userDataAccess.ResetFailedAttempts(userId);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="lockoutEnd">The lockoutEnd value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
    {
        return _userDataAccess.LockAccount(userId, lockoutEnd);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        return _userDataAccess.UpdatePassword(userId, newPasswordHash);
    }
}
