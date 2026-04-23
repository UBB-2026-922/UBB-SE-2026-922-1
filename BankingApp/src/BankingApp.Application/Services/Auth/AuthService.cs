// <copyright file="AuthService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AuthService class.
// </summary>

using System.Security.Cryptography;
using System.Text;
using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Notifications;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Auth;

/// <summary>
///     Provides authentication, registration, OTP verification, and password management operations.
/// </summary>
public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int PasswordResetTokenExpiryMinutes = 30;
    private const int PasswordResetTokenByteLength = 32;
    private const int FailedLoginAttemptIncrement = 1;
    private const string GoogleOAuthProvider = "Google";
    private const string DefaultLanguage = "en";
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;
    private readonly IJsonWebTokenService _jsonWebTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IOtpService _otpService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthService" /> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jsonWebTokenService">The JWT token service.</param>
    /// <param name="otpService">The OTP service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="logger">The _logger.</param>
    public AuthService(
        IAuthRepository authRepository,
        IHashService hashService,
        IJsonWebTokenService jsonWebTokenService,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _hashService = hashService;
        _jsonWebTokenService = jsonWebTokenService;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<LoginSuccess> Login(LoginRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return AuthErrors.InvalidEmail;
        }

        ErrorOr<User> userResult = _authRepository.FindUserByEmail(request.Email);
        if (userResult.IsError)
        {
            _logger.LogWarning("Login failed: user not found for email.");
            return AuthErrors.InvalidCredentials;
        }

        User user = userResult.Value;
        Error? lockError = CheckAccountLock(user);
        if (lockError is not null)
        {
            return lockError.Value;
        }

        if (user.PasswordHash is null)
        {
            _logger.LogWarning("Login with password rejected for OAuth-only account {UserId}.", user.Id);
            // Use the same response as any other bad password to avoid exposing account auth methods.
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<bool> verifyResult = _hashService.Verify(request.Password, user.PasswordHash);
        if (verifyResult.IsError)
        {
            _logger.LogError(
                "Password hash verification threw for user {UserId}: {Error}",
                user.Id,
                verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return HandleFailedPassword(user);
        }

        return user.Is2FaEnabled ? Handle2Fa(user) : CompleteLogin(user);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Register(RegisterRequest request)
    {
        Error? validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return validationError.Value;
        }

        if (!_authRepository.FindUserByEmail(request.Email).IsError)
        {
            _logger.LogInformation("Registration rejected: email already registered.");
            return AuthErrors.EmailAlreadyRegistered;
        }

        ErrorOr<User> newUserResult = CreateUserFromRequest(request);
        if (newUserResult.IsError)
        {
            return newUserResult.FirstError;
        }

        if (_authRepository.CreateUser(newUserResult.Value).IsError)
        {
            _logger.LogError("User creation failed during registration.");
            return UserErrors.UserCreationFailed;
        }

        _logger.LogInformation("User registered successfully.");
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<ErrorOr<LoginSuccess>> OAuthLoginAsync(OAuthLoginRequest request)
    {
        if (!request.Provider.Equals(GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase))
        {
            return AuthErrors.UnsupportedProvider;
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.ProviderToken);
        }
        catch (InvalidJwtException)
        {
            _logger.LogWarning("OAuth login rejected: invalid Google token.");
            return AuthErrors.InvalidGoogleToken;
        }

        string providerUserId = payload.Subject;
        string email = payload.Email;
        string fullName = payload.Name;
        ErrorOr<OAuthLink> linkResult = _authRepository.FindOAuthLink(request.Provider, providerUserId);
        User? user = null;
        if (!linkResult.IsError)
        {
            ErrorOr<User> userResult = _authRepository.FindUserById(linkResult.Value.UserId);
            if (!userResult.IsError)
            {
                user = userResult.Value;
            }
        }

        if (user is null)
        {
            ErrorOr<User> byEmailResult = _authRepository.FindUserByEmail(email);
            if (!byEmailResult.IsError)
            {
                user = byEmailResult.Value;
            }
            else
            {
                var newUser = new User
                {
                    Email = email,
                    FullName = fullName,
                    PreferredLanguage = DefaultLanguage,
                    Is2FaEnabled = false,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                };
                if (_authRepository.CreateUser(newUser).IsError)
                {
                    _logger.LogError("OAuth user creation failed for provider {Provider}.", request.Provider);
                    return UserErrors.UserCreationFailed;
                }

                ErrorOr<User> createdResult = _authRepository.FindUserByEmail(email);
                if (createdResult.IsError)
                {
                    _logger.LogError(
                        "Failed to retrieve user after OAuth creation for provider {Provider}.",
                        request.Provider);
                    return UserErrors.UserRetrievalFailed;
                }

                user = createdResult.Value;
            }

            var newLink = new OAuthLink
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = providerUserId,
                ProviderEmail = email,
            };
            if (_authRepository.CreateOAuthLink(newLink).IsError)
            {
                _logger.LogError(
                    "Failed to create OAuth link for user {UserId}, provider {Provider}.",
                    user.Id,
                    request.Provider);
                return UserErrors.OAuthLinkFailed;
            }
        }

        Error? lockError = CheckAccountLock(user);
        if (lockError is not null)
        {
            return lockError.Value;
        }

        return user.Is2FaEnabled ? Handle2Fa(user) : CompleteLogin(user);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> OAuthRegister(OAuthRegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return AuthErrors.InvalidEmail;
        }

        if (!_authRepository.FindOAuthLink(request.Provider, request.ProviderToken).IsError)
        {
            return AuthErrors.OAuthAlreadyRegistered;
        }

        int targetUserId;
        ErrorOr<User> existingUserResult = _authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            targetUserId = existingUserResult.Value.Id;
        }
        else
        {
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = null,
                FullName = request.FullName,
                PreferredLanguage = DefaultLanguage,
                Is2FaEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0,
            };
            if (_authRepository.CreateUser(newUser).IsError)
            {
                return UserErrors.UserCreationFailed;
            }

            ErrorOr<User> savedUserResult = _authRepository.FindUserByEmail(request.Email);
            if (savedUserResult.IsError)
            {
                return UserErrors.UserRetrievalFailed;
            }

            targetUserId = savedUserResult.Value.Id;
        }

        var newLink = new OAuthLink
        {
            UserId = targetUserId,
            Provider = request.Provider,
            ProviderUserId = request.ProviderToken,
            ProviderEmail = request.Email,
        };
        if (_authRepository.CreateOAuthLink(newLink).IsError)
        {
            return UserErrors.OAuthLinkFailed;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<LoginSuccess> VerifyOtp(VerifyOtpRequest request)
    {
        ErrorOr<User> userResult = _authRepository.FindUserById(request.UserId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OTP verification failed: user {UserId} not found.", request.UserId);
            return AuthErrors.UserNotFound;
        }

        User user = userResult.Value;
        ErrorOr<bool> verifyResult = _otpService.VerifyTotp(request.UserId, request.OtpCode);
        if (verifyResult.IsError)
        {
            _logger.LogError(
                "TOTP verification threw for user {UserId}: {Error}",
                user.Id,
                verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            _logger.LogWarning("OTP verification failed for user {UserId}: invalid or expired code.", user.Id);
            return AuthErrors.InvalidOtp;
        }

        _otpService.InvalidateOtp(user.Id);
        return CompleteLogin(user);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="method">The method value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> ResendOtp(int userId, string method)
    {
        ErrorOr<User> userResult = _authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OTP resend failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        ErrorOr<string> otpResult = _otpService.GenerateTotp(user.Id);
        if (otpResult.IsError)
        {
            _logger.LogError(
                "TOTP generation failed during resend for user {UserId}: {Error}",
                user.Id,
                otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (string.Equals(method, nameof(TwoFactorMethod.Email), StringComparison.OrdinalIgnoreCase)
            || user.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            _emailService.SendOtpCode(user.Email, otpResult.Value);
        }

        return Result.Success;
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

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Logout(string token)
    {
        ErrorOr<Session> sessionResult = _authRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            _logger.LogWarning("Logout failed: session not found.");
            return sessionResult.FirstError;
        }

        _ = _authRepository.UpdateSessionToken(sessionResult.Value.Id);
        _logger.LogInformation("User {UserId} logged out.", sessionResult.Value.UserId);
        return Result.Success;
    }

    private Error? CheckAccountLock(User user)
    {
        if (!user.IsLocked)
        {
            return null;
        }

        if (user.IsCurrentlyLocked())
        {
            _logger.LogWarning(
                "Login blocked: account {UserId} is locked until {LockoutEnd}.",
                user.Id,
                user.LockoutEnd);
            return AuthErrors.AccountLocked;
        }

        _ = _authRepository.ResetFailedAttempts(user.Id);
        return null;
    }

    private Error HandleFailedPassword(User user)
    {
        _ = _authRepository.IncrementFailedAttempts(user.Id);
        int failedAttemptsAfterCurrentFailure = user.FailedLoginAttempts + FailedLoginAttemptIncrement;
        _logger.LogWarning(
            "Failed login attempt for user {UserId}. Attempt {Attempt}/{Max}.",
            user.Id,
            failedAttemptsAfterCurrentFailure,
            MaxFailedAttempts);
        if (failedAttemptsAfterCurrentFailure < MaxFailedAttempts)
        {
            return AuthErrors.InvalidCredentials;
        }

        if (_authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes)).IsError)
        {
            _logger.LogError(
                "Failed to lock account {UserId} after {Max} failed attempts.",
                user.Id,
                MaxFailedAttempts);
            return AuthErrors.TooManyFailedAttempts;
        }

        _logger.LogWarning(
            "Account {UserId} locked for {Minutes} minutes after {Max} failed attempts.",
            user.Id,
            LockoutMinutes,
            MaxFailedAttempts);
        _emailService.SendLockNotification(user.Email);
        return AuthErrors.AccountLockedTooManyAttempts;
    }

    private ErrorOr<LoginSuccess> Handle2Fa(User user)
    {
        ErrorOr<string> otpResult = _otpService.GenerateTotp(user.Id);
        if (otpResult.IsError)
        {
            _logger.LogError(
                "TOTP generation failed for user {UserId}: {Error}",
                user.Id,
                otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (user.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            _emailService.SendOtpCode(user.Email, otpResult.Value);
        }

        _logger.LogInformation("2FA required for user {UserId} via {Method}.", user.Id, user.Preferred2FaMethod);
        return new RequiresTwoFactor(user.Id);
    }

    private ErrorOr<LoginSuccess> CompleteLogin(User user)
    {
        _ = _authRepository.ResetFailedAttempts(user.Id);
        ErrorOr<string> tokenResult = _jsonWebTokenService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            _logger.LogError(
                "Token generation failed for user {UserId}: {Error}",
                user.Id,
                tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        if (_authRepository.CreateSession(user.Id, token, null, null, null).IsError)
        {
            _logger.LogError("Session creation failed for user {UserId}.", user.Id);
            return UserErrors.SessionCreationFailed;
        }

        _logger.LogInformation("User {UserId} logged in successfully.", user.Id);
        _emailService.SendLoginAlert(user.Email);
        return new FullLogin(user.Id, token);
    }

    private Error? ValidateRegistration(RegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return AuthErrors.InvalidEmail;
        }

        if (!ValidationUtilities.IsStrongPassword(request.Password))
        {
            return ProfileErrors.WeakPassword;
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return ProfileErrors.FullNameRequired;
        }

        return null;
    }

    private ErrorOr<User> CreateUserFromRequest(RegisterRequest request)
    {
        ErrorOr<string> hashResult = _hashService.GetHash(request.Password);
        if (hashResult.IsError)
        {
            _logger.LogError("Hash generation failed during registration.");
            return hashResult.FirstError;
        }

        return new User
        {
            Email = request.Email,
            PasswordHash = hashResult.Value,
            FullName = request.FullName,
            PreferredLanguage = DefaultLanguage,
            Is2FaEnabled = false,
            IsLocked = false,
            FailedLoginAttempts = 0,
        };
    }

    private ErrorOr<Success> ValidateResetToken(PasswordResetToken resetToken)
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
