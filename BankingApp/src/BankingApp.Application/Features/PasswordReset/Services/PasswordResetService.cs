namespace BankingApp.Application.Features.PasswordReset.Services;

using BankingApp.Application.Features.Authentication.Repositories;
using System.Security.Cryptography;
using System.Text;
using BankingApp.Application.Common.Logging;

using BankingApp.Application.Common.Notifications;
using BankingApp.Application.Common.Security;
using Domain.Common.Errors;
using Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides password reset and recovery operations.
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private const int PasswordResetTokenExpiryMinutes = 30;
    private const int PasswordResetTokenByteLength = 32;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;
    private readonly ILogger<PasswordResetService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordResetService" /> class.
    /// </summary>
    /// <param name="authenticationRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="logger">The _logger.</param>
    public PasswordResetService(
        IAuthenticationRepository authenticationRepository,
        IHashService hashService,
        IEmailService emailService,
        ILogger<PasswordResetService> logger)
    {
        _authenticationRepository = authenticationRepository;
        _hashService = hashService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RequestPasswordReset(string email)
    {
        ErrorOr<User> userResult = _authenticationRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
            _logger.PasswordResetNoAccountFound();
            return userResult.FirstError;
        }

        User user = userResult.Value;
        _ = _authenticationRepository.DeleteExpiredPasswordResetTokens();
        byte[] randomBytes = RandomNumberGenerator.GetBytes(PasswordResetTokenByteLength);
        string rawToken = Convert.ToBase64String(randomBytes);
        string tokenHashForDb = ComputeSha256Hash(rawToken);
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHashForDb,
            ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            CreatedAt = DateTime.UtcNow
        };
        if (_authenticationRepository.SavePasswordResetToken(resetToken).IsError)
        {
            _logger.PasswordResetSaveTokenFailed(user.Id);
            return PasswordResetErrors.SaveTokenFailed;
        }

        _logger.PasswordResetEmailSent(user.Id);
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
        ErrorOr<PasswordResetToken> tokenResult = _authenticationRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            _logger.PasswordResetTokenNotFound();
            return PasswordResetErrors.TokenInvalid;
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ErrorOr<Success> validationResult = ValidateResetToken(resetToken);
        if (validationResult.IsError)
        {
            _logger.PasswordResetValidationFailed(resetToken.UserId, validationResult.FirstError.Code);
            return validationResult.FirstError;
        }

        ErrorOr<string> hashResult = _hashService.GetHash(newPassword);
        if (hashResult.IsError)
        {
            _logger.PasswordResetHashGenerationFailed(resetToken.UserId);
            return hashResult.FirstError;
        }

        if (_authenticationRepository.UpdatePassword(resetToken.UserId, hashResult.Value).IsError)
        {
            _logger.PasswordUpdateFailed(resetToken.UserId);
            return PasswordResetErrors.TokenInvalid;
        }

        if (_authenticationRepository.MarkPasswordResetTokenAsUsed(resetToken.Id).IsError)
        {
            _logger.PasswordResetMarkUsedFailed(resetToken.UserId);
            return PasswordResetErrors.ResetFailedTokenNotInvalidated;
        }

        if (_authenticationRepository.InvalidateAllSessions(resetToken.UserId).IsError)
        {
            _logger.PasswordResetInvalidateSessionsFailed(resetToken.UserId);
            return PasswordResetErrors.ResetFailedSessionsNotInvalidated;
        }

        _logger.PasswordResetSucceeded(resetToken.UserId);
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
        ErrorOr<PasswordResetToken> tokenResult = _authenticationRepository.FindPasswordResetToken(tokenHash);
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

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
