// <copyright file="PasswordRecoveryService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the PasswordRecoveryService class.
// </summary>

using System.Security.Cryptography;
using System.Text;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using BankApp.Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.PasswordRecovery;

/// <summary>
///     Provides password reset and recovery operations.
/// </summary>
public class PasswordRecoveryService : IPasswordRecoveryService
{
    private const int PasswordResetTokenExpiryMinutes = 30;
    private const int PasswordResetTokenByteLength = 32;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;
    private readonly ILogger<PasswordRecoveryService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordRecoveryService" /> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="logger">The _logger.</param>
    public PasswordRecoveryService(
        IAuthRepository authRepository,
        IHashService hashService,
        IEmailService emailService,
        ILogger<PasswordRecoveryService> logger)
    {
        _authRepository = authRepository;
        _hashService = hashService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RequestPasswordReset(string email)
    {
        ErrorOr<User> userResult = _authRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
            _logger.LogInformation("Password reset requested: no account found.");
            return userResult.FirstError;
        }

        User user = userResult.Value;
        _ = _authRepository.DeleteExpiredPasswordResetTokens();
        byte[] randomBytes = RandomNumberGenerator.GetBytes(PasswordResetTokenByteLength);
        string rawToken = Convert.ToBase64String(randomBytes);
        string tokenHashForDb = ComputeSha256Hash(rawToken);
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHashForDb,
            ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            CreatedAt = DateTime.UtcNow,
        };
        if (_authRepository.SavePasswordResetToken(resetToken).IsError)
        {
            _logger.LogError("Failed to save password reset token for user {UserId}.", user.Id);
            return PasswordResetErrors.SaveTokenFailed;
        }

        _logger.LogInformation("Password reset email sent for user {UserId}.", user.Id);
        _emailService.SendPasswordResetLink(user.Email, rawToken);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <param name="newPassword">The newPassword value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> ResetPassword(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return PasswordResetErrors.TokenInvalid;
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = _authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            _logger.LogWarning("Password reset failed: token not found.");
            return PasswordResetErrors.TokenInvalid;
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ErrorOr<Success> validationResult = ValidateResetToken(resetToken);
        if (validationResult.IsError)
        {
            _logger.LogWarning(
                "Password reset failed for user {UserId}: {Code}.",
                resetToken.UserId,
                validationResult.FirstError.Code);
            return validationResult.FirstError;
        }

        ErrorOr<string> hashResult = _hashService.GetHash(newPassword);
        if (hashResult.IsError)
        {
            _logger.LogError("Hash generation failed during password reset for user {UserId}.", resetToken.UserId);
            return hashResult.FirstError;
        }

        if (_authRepository.UpdatePassword(resetToken.UserId, hashResult.Value).IsError)
        {
            _logger.LogError("Password update failed for user {UserId}.", resetToken.UserId);
            return PasswordResetErrors.TokenInvalid;
        }

        if (_authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id).IsError)
        {
            _logger.LogError(
                "Failed to mark password reset token as used for user {UserId}. Token may be replayable.",
                resetToken.UserId);
            return PasswordResetErrors.ResetFailedTokenNotInvalidated;
        }

        if (_authRepository.InvalidateAllSessions(resetToken.UserId).IsError)
        {
            _logger.LogError(
                "Failed to invalidate sessions for user {UserId} after password reset. Active sessions may remain valid.",
                resetToken.UserId);
            return PasswordResetErrors.ResetFailedSessionsNotInvalidated;
        }

        _logger.LogInformation("Password reset successfully for user {UserId}.", resetToken.UserId);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> VerifyResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return PasswordResetErrors.TokenInvalid;
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = _authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        return ValidateResetToken(tokenResult.Value);
    }

    private static ErrorOr<Success> ValidateResetToken(PasswordResetToken resetToken)
    {
        if (resetToken.UsedAt != null)
        {
            return PasswordResetErrors.TokenAlreadyUsed;
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return PasswordResetErrors.TokenExpired;
        }

        return Result.Success;
    }

    private string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}