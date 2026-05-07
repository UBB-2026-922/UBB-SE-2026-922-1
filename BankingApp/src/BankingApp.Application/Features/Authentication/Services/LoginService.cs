namespace BankingApp.Application.Features.Authentication.Services;

using BankingApp.Application.Features.Authentication.Repositories;
using BankingApp.Application.Features.Authentication.Models;
using BankingApp.Application.Common.Logging;

using BankingApp.Application.Common.Notifications;
using BankingApp.Application.Common.Security;
using BankingApp.Application.Common.Utilities;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

using BankingApp.Application.Features.Authentication.Dtos;
using Domain.Common.Errors;

/// <summary>
///     Provides login, logout, OAuth login, and 2FA operations.
/// </summary>
public class LoginService : ILoginService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int MaxFailedOtpAttempts = 3;
    private const int FailedLoginAttemptIncrement = 1;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;
    private readonly IJsonWebTokenService _jsonWebTokenService;
    private readonly ILogger<LoginService> _logger;
    private readonly IOtpAttemptTracker _otpAttemptTracker;
    private readonly IOtpService _otpService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginService" /> class.
    /// </summary>
    /// <param name="authenticationRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jsonWebTokenService">The JWT token service.</param>
    /// <param name="otpService">The OTP service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="otpAttemptTracker">The OTP failure attempt tracker.</param>
    /// <param name="logger">The _logger.</param>
    public LoginService(
        IAuthenticationRepository authenticationRepository,
        IHashService hashService,
        IJsonWebTokenService jsonWebTokenService,
        IOtpService otpService,
        IEmailService emailService,
        IOtpAttemptTracker otpAttemptTracker,
        ILogger<LoginService> logger)
    {
        _authenticationRepository = authenticationRepository;
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

        ErrorOr<User> userResult = _authenticationRepository.FindUserByEmail(request.Email);
        if (userResult.IsError)
        {
            _logger.LoginUserNotFoundForEmail();
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
            _logger.LoginOAuthOnlyPasswordRejected(user.Id);
            // Use the same response as any other bad password to avoid exposing account auth methods.
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<bool> verifyResult = _hashService.Verify(request.Password, user.PasswordHash);
        if (verifyResult.IsError)
        {
            _logger.PasswordHashVerificationFailed(user.Id, verifyResult.FirstError.Description);
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
    public ErrorOr<LoginSuccess> VerifyOtp(VerifyOtpRequest request, SessionMetadata? metadata = null)
    {
        ErrorOr<User> userResult = _authenticationRepository.FindUserById(request.UserId);
        if (userResult.IsError)
        {
            _logger.OtpVerificationUserNotFound(request.UserId);
            return AuthErrors.UserNotFound;
        }

        User user = userResult.Value;
        ErrorOr<bool> verifyResult = VerifyOtpForPreferredMethod(user, request.OtpCode);
        if (verifyResult.IsError)
        {
            _logger.OtpVerificationFailed(user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            _logger.OtpVerificationInvalidOrExpired(user.Id);
            if (_otpAttemptTracker.RecordFailure(user.Id) < MaxFailedOtpAttempts)
            {
                return AuthErrors.InvalidOtp;
            }

            _otpService.InvalidateOtp(user.Id);
            _otpAttemptTracker.Reset(user.Id);
            _logger.OtpChallengeInvalidated(user.Id, MaxFailedOtpAttempts);
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
        ErrorOr<User> userResult = _authenticationRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            _logger.OtpResendUserNotFound(userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            _logger.OtpGenerationDuringResendFailed(user.Id, otpResult.FirstError.Description);
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
        ErrorOr<Session> sessionResult = _authenticationRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            _logger.LogoutSessionNotFound();
            return sessionResult.FirstError;
        }

        _ = _authenticationRepository.UpdateSessionToken(sessionResult.Value.Id);
        _logger.UserLoggedOut(sessionResult.Value.UserId);
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
            _logger.LoginBlockedLockedAccount(user.Id, user.LockoutEnd);
            return AuthErrors.AccountLocked;
        }

        _ = _authenticationRepository.ResetFailedAttempts(user.Id);
        return null;
    }

    private Error HandleFailedPassword(User user)
    {
        _ = _authenticationRepository.IncrementFailedAttempts(user.Id);
        int failedAttemptsAfterCurrentFailure = user.FailedLoginAttempts + FailedLoginAttemptIncrement;
        _logger.FailedLoginAttempt(user.Id, failedAttemptsAfterCurrentFailure, MaxFailedAttempts);
        if (failedAttemptsAfterCurrentFailure < MaxFailedAttempts)
        {
            return AuthErrors.InvalidCredentials;
        }

        if (_authenticationRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes)).IsError)
        {
            _logger.FailedToLockAccount(user.Id, MaxFailedAttempts);
            return AuthErrors.TooManyFailedAttempts;
        }

        _logger.AccountLockedTooManyAttempts(user.Id, LockoutMinutes, MaxFailedAttempts);
        _emailService.SendLockNotification(user.Email);
        return AuthErrors.AccountLockedTooManyAttempts;
    }

    private ErrorOr<LoginSuccess> Handle2Fa(User user)
    {
        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            _logger.OtpGenerationFailed(user.Id, otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (user.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            _emailService.SendOtpCode(user.Email, otpResult.Value);
        }

        _otpAttemptTracker.Reset(user.Id);
        _logger.TwoFactorRequired(user.Id, user.Preferred2FaMethod);
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
        _ = _authenticationRepository.ResetFailedAttempts(user.Id);
        ErrorOr<string> tokenResult = _jsonWebTokenService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            _logger.TokenGenerationFailed(user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        if (_authenticationRepository.CreateSession(user.Id, token, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress)
            .IsError)
        {
            _logger.SessionCreationFailed(user.Id);
            return UserErrors.SessionCreationFailed;
        }

        _logger.UserLoggedIn(user.Id);
        _emailService.SendLoginAlert(user.Email);
        return new FullLogin(user.Id, token);
    }
}
