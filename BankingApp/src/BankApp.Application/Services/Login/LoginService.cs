// <copyright file="LoginService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the LoginService class.
// </summary>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Application.Utilities;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using BankApp.Domain.Errors;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Login;

/// <summary>
///     Provides login, logout, OAuth login, and 2FA operations.
/// </summary>
public class LoginService : ILoginService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int MaxFailedOtpAttempts = 3;
    private const int FailedLoginAttemptIncrement = 1;
    private const string GoogleOAuthProvider = "Google";
    private const string DefaultLanguage = "en";
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;
    private readonly IJsonWebTokenService _jsonWebTokenService;
    private readonly ILogger<LoginService> _logger;
    private readonly IOtpAttemptTracker _otpAttemptTracker;
    private readonly IOtpService _otpService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginService" /> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jsonWebTokenService">The JWT token service.</param>
    /// <param name="otpService">The OTP service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="otpAttemptTracker">The OTP failure attempt tracker.</param>
    /// <param name="logger">The _logger.</param>
    public LoginService(
        IAuthRepository authRepository,
        IHashService hashService,
        IJsonWebTokenService jsonWebTokenService,
        IOtpService otpService,
        IEmailService emailService,
        IOtpAttemptTracker otpAttemptTracker,
        ILogger<LoginService> logger)
    {
        _authRepository = authRepository;
        _hashService = hashService;
        _jsonWebTokenService = jsonWebTokenService;
        _otpService = otpService;
        _emailService = emailService;
        _otpAttemptTracker = otpAttemptTracker;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <param name="metadata">The metadata value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<LoginSuccess> Login(LoginRequest request, SessionMetadata? metadata = null)
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

        return user.Is2FaEnabled ? Handle2Fa(user) : CompleteLogin(user, metadata);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <param name="metadata">The metadata value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<ErrorOr<LoginSuccess>> OAuthLoginAsync(
        OAuthLoginRequest request,
        SessionMetadata? metadata = null)
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

        return user.Is2FaEnabled ? Handle2Fa(user) : CompleteLogin(user, metadata);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <param name="metadata">The metadata value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<LoginSuccess> VerifyOtp(VerifyOtpRequest request, SessionMetadata? metadata = null)
    {
        ErrorOr<User> userResult = _authRepository.FindUserById(request.UserId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OTP verification failed: user {UserId} not found.", request.UserId);
            return AuthErrors.UserNotFound;
        }

        User user = userResult.Value;
        ErrorOr<bool> verifyResult = VerifyOtpForPreferredMethod(user, request.OtpCode);
        if (verifyResult.IsError)
        {
            _logger.LogError(
                "OTP verification threw for user {UserId}: {Error}",
                user.Id,
                verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            _logger.LogWarning("OTP verification failed for user {UserId}: invalid or expired code.", user.Id);
            if (_otpAttemptTracker.RecordFailure(user.Id) < MaxFailedOtpAttempts)
            {
                return AuthErrors.InvalidOtp;
            }

            _otpService.InvalidateOtp(user.Id);
            _otpAttemptTracker.Reset(user.Id);
            _logger.LogWarning(
                "OTP challenge invalidated for user {UserId} after {MaxAttempts} failed attempts.",
                user.Id,
                MaxFailedOtpAttempts);
            return AuthErrors.OtpAttemptsExceeded;

        }

        _otpAttemptTracker.Reset(user.Id);
        _otpService.InvalidateOtp(user.Id);
        return CompleteLogin(user, metadata);
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
        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            _logger.LogError(
                "OTP generation failed during resend for user {UserId}: {Error}",
                user.Id,
                otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (string.Equals(method, nameof(TwoFactorMethod.Email), StringComparison.OrdinalIgnoreCase)
            || user.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            _emailService.SendOtpCode(user.Email, otpResult.Value);
        }

        _otpAttemptTracker.Reset(user.Id);
        return Result.Success;
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
        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            _logger.LogError(
                "OTP generation failed for user {UserId}: {Error}",
                user.Id,
                otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (user.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            _emailService.SendOtpCode(user.Email, otpResult.Value);
        }

        _otpAttemptTracker.Reset(user.Id);
        _logger.LogInformation("2FA required for user {UserId} via {Method}.", user.Id, user.Preferred2FaMethod);
        return new RequiresTwoFactor(user.Id);
    }

    private ErrorOr<string> GenerateOtpForMethod(User user)
    {
        return user.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? _otpService.GenerateTotp(user.Id)
            : _otpService.GenerateSmsOtp(user.Id);
    }

    private ErrorOr<bool> VerifyOtpForPreferredMethod(User user, string code)
    {
        return user.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? _otpService.VerifyTotp(user.Id, code)
            : _otpService.VerifySmsOtp(user.Id, code);
    }

    private ErrorOr<LoginSuccess> CompleteLogin(User user, SessionMetadata? metadata)
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
        if (_authRepository.CreateSession(user.Id, token, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress)
            .IsError)
        {
            _logger.LogError("Session creation failed for user {UserId}.", user.Id);
            return UserErrors.SessionCreationFailed;
        }

        _logger.LogInformation("User {UserId} logged in successfully.", user.Id);
        _emailService.SendLoginAlert(user.Email);
        return new FullLogin(user.Id, token);
    }
}
