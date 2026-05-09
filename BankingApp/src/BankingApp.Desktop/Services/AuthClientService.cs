namespace BankingApp.Desktop.Services;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Repositories.Interfaces;
using Application.Services.Login;
using Application.Utilities;
using BankingApp.Desktop.ProxyRepositories;
using BankingApp.Desktop.Utilities;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Owns the desktop authentication business logic and persists data through proxy repositories.
/// </summary>
internal sealed class AuthClientService : IAuthClientService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int MaxFailedOtpAttempts = 3;
    private const int PasswordResetTokenExpiryMinutes = 30;
    private const int PasswordResetTokenByteLength = 32;
    private readonly IApiClient _apiClient;
    private readonly IAuthRepository _authRepository;
    private readonly ILogger<AuthClientService> _logger;
    private readonly IOtpAttemptTracker _otpAttemptTracker;
    private readonly SecurityProxyRepository _securityProxyRepository;

    public AuthClientService(
        IApiClient apiClient,
        IAuthRepository authRepository,
        SecurityProxyRepository securityProxyRepository,
        IOtpAttemptTracker otpAttemptTracker,
        ILogger<AuthClientService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _securityProxyRepository = securityProxyRepository ?? throw new ArgumentNullException(nameof(securityProxyRepository));
        _otpAttemptTracker = otpAttemptTracker ?? throw new ArgumentNullException(nameof(otpAttemptTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int? CurrentUserId
    {
        get => _apiClient.CurrentUserId;
        set => _apiClient.CurrentUserId = value;
    }

    public ErrorOr<Success> EnsureConfigured() => _apiClient.EnsureConfigured();

    public void SetToken(string token) => _apiClient.SetToken(token);

    public async Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password)
    {
        if (!ValidationUtilities.IsValidEmail(email))
        {
            return AuthErrors.InvalidEmail;
        }

        ErrorOr<User> userResult = _authRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
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
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<bool> verifyResult = await _securityProxyRepository.VerifyHashAsync(password, user.PasswordHash);
        if (verifyResult.IsError)
        {
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return await HandleFailedPasswordAsync(user);
        }

        return user.Is2FaEnabled ? await HandleTwoFactorAsync(user) : await CompleteLoginAsync(user);
    }

    public async Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName)
    {
        if (!ValidationUtilities.IsValidEmail(email))
        {
            return AuthErrors.InvalidEmail;
        }

        if (!IsPasswordValid(password))
        {
            return ProfileErrors.WeakPassword;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ProfileErrors.FullNameRequired;
        }

        ErrorOr<User> existingUserResult = _authRepository.FindUserByEmail(email);
        if (!existingUserResult.IsError)
        {
            return AuthErrors.EmailAlreadyRegistered;
        }

        if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            return UserErrors.DatabaseError;
        }

        ErrorOr<string> hashResult = await _securityProxyRepository.HashAsync(password);
        if (hashResult.IsError)
        {
            return hashResult.FirstError;
        }

        return _authRepository.CreateUser(
            new User
            {
                Email = email,
                PasswordHash = hashResult.Value,
                FullName = fullName,
                PreferredLanguage = User.DefaultPreferredLanguage,
                Is2FaEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0,
            });
    }

    public async Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode)
    {
        ErrorOr<User> userResult = _authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            return AuthErrors.UserNotFound;
        }

        User user = userResult.Value;
        ErrorOr<bool> verifyResult = await VerifyOtpForPreferredMethodAsync(user, otpCode);
        if (verifyResult.IsError)
        {
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            int failures = _otpAttemptTracker.RecordFailure(user.Id);
            if (failures >= MaxFailedOtpAttempts)
            {
                _otpAttemptTracker.Reset(user.Id);
                await _securityProxyRepository.InvalidateOtpAsync(user.Id);
                return AuthErrors.OtpAttemptsExceeded;
            }

            return AuthErrors.InvalidOtp;
        }

        _otpAttemptTracker.Reset(user.Id);
        await _securityProxyRepository.InvalidateOtpAsync(user.Id);
        return await CompleteLoginAsync(user);
    }

    public async Task<ErrorOr<object>> ResendOtpAsync(int userId)
    {
        ErrorOr<User> userResult = _authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            return new object();
        }

        User user = userResult.Value;
        ErrorOr<string> otpResult = await GenerateOtpForMethodAsync(user);
        if (otpResult.IsError)
        {
            return otpResult.FirstError;
        }

        if (ShouldSendEmailOtp(user))
        {
            ErrorOr<Success> emailResult = await _securityProxyRepository.SendOtpCodeAsync(user.Email, otpResult.Value);
            if (emailResult.IsError)
            {
                return emailResult.FirstError;
            }
        }

        _otpAttemptTracker.Reset(user.Id);
        return new object();
    }

    public async Task<ErrorOr<Success>> RequestPasswordResetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Error.Validation("email_required", "Email is required.");
        }

        ErrorOr<User> userResult = _authRepository.FindUserByEmail(email.Trim());
        if (userResult.IsError)
        {
            return Result.Success;
        }

        _ = _authRepository.DeleteExpiredPasswordResetTokens();

        User user = userResult.Value;
        byte[] randomBytes = RandomNumberGenerator.GetBytes(PasswordResetTokenByteLength);
        string rawToken = Convert.ToBase64String(randomBytes);
        ErrorOr<Success> saveResult = _authRepository.SavePasswordResetToken(
            new PasswordResetToken
            {
                User = user,
                TokenHash = ComputeSha256Hash(rawToken),
                ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
                CreatedAt = DateTime.UtcNow,
            });
        if (saveResult.IsError)
        {
            return PasswordResetErrors.SaveTokenFailed;
        }

        ErrorOr<Success> emailResult = await _securityProxyRepository.SendPasswordResetLinkAsync(user.Email, rawToken);
        return emailResult.IsError ? emailResult.FirstError : Result.Success;
    }

    public Task<ErrorOr<Success>> VerifyResetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<ErrorOr<Success>>(PasswordResetErrors.TokenInvalid);
        }

        ErrorOr<PasswordResetToken> tokenResult = _authRepository.FindPasswordResetToken(ComputeSha256Hash(token));
        return Task.FromResult(tokenResult.IsError ? (ErrorOr<Success>)PasswordResetErrors.TokenInvalid : ValidateResetToken(tokenResult.Value));
    }

    public async Task<ErrorOr<Success>> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return PasswordResetErrors.TokenInvalid;
        }

        if (!IsPasswordValid(newPassword))
        {
            return ProfileErrors.WeakPassword;
        }

        ErrorOr<PasswordResetToken> tokenResult = _authRepository.FindPasswordResetToken(ComputeSha256Hash(token));
        if (tokenResult.IsError)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ErrorOr<Success> validationResult = ValidateResetToken(resetToken);
        if (validationResult.IsError)
        {
            return validationResult.FirstError;
        }

        ErrorOr<string> hashResult = await _securityProxyRepository.HashAsync(newPassword);
        if (hashResult.IsError)
        {
            return hashResult.FirstError;
        }

        int userId = resetToken.User?.Id ?? 0;
        ErrorOr<Success> updatePasswordResult = _authRepository.UpdatePassword(userId, hashResult.Value);
        if (updatePasswordResult.IsError)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        ErrorOr<Success> markUsedResult = _authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id);
        if (markUsedResult.IsError)
        {
            return PasswordResetErrors.ResetFailedTokenNotInvalidated;
        }

        ErrorOr<Success> invalidateSessionsResult = _authRepository.InvalidateAllSessions(userId);
        return invalidateSessionsResult.IsError
            ? PasswordResetErrors.ResetFailedSessionsNotInvalidated
            : Result.Success;
    }

    public bool IsPasswordValid(string password) => PasswordValidator.IsStrong(password);

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
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

    private static bool ShouldSendEmailOtp(User user)
        => user.Preferred2FaMethod != TwoFactorMethod.Authenticator;

    private Error? CheckAccountLock(User user)
    {
        if (!user.IsLocked)
        {
            return null;
        }

        if (user.IsCurrentlyLocked())
        {
            return AuthErrors.AccountLocked;
        }

        ErrorOr<Success> resetResult = _authRepository.ResetFailedAttempts(user.Id);
        if (resetResult.IsError)
        {
            _logger.LogWarning(
                "Failed to reset expired lockout state for user {UserId}: {Description}",
                user.Id,
                resetResult.FirstError.Description);
        }

        return null;
    }

    private async Task<ErrorOr<LoginSuccessResponse>> HandleTwoFactorAsync(User user)
    {
        ErrorOr<string> otpResult = await GenerateOtpForMethodAsync(user);
        if (otpResult.IsError)
        {
            return otpResult.FirstError;
        }

        if (ShouldSendEmailOtp(user))
        {
            ErrorOr<Success> emailResult = await _securityProxyRepository.SendOtpCodeAsync(user.Email, otpResult.Value);
            if (emailResult.IsError)
            {
                return emailResult.FirstError;
            }
        }

        _otpAttemptTracker.Reset(user.Id);
        return new LoginSuccessResponse { UserId = user.Id, Requires2Fa = true };
    }

    private async Task<ErrorOr<LoginSuccessResponse>> CompleteLoginAsync(User user)
    {
        _ = _authRepository.ResetFailedAttempts(user.Id);

        ErrorOr<string> tokenResult = await _securityProxyRepository.GenerateTokenAsync(user.Id);
        if (tokenResult.IsError)
        {
            return tokenResult.FirstError;
        }

        ErrorOr<Session> sessionResult = _authRepository.CreateSession(
            user.Id,
            tokenResult.Value,
            Environment.MachineName,
            "BankingApp Desktop",
            null);
        if (sessionResult.IsError)
        {
            return UserErrors.SessionCreationFailed;
        }

        ErrorOr<Success> alertResult = await _securityProxyRepository.SendLoginAlertAsync(user.Email);
        if (alertResult.IsError)
        {
            _logger.LogWarning(
                "Login succeeded for user {UserId}, but the login alert email failed: {Description}",
                user.Id,
                alertResult.FirstError.Description);
        }

        return new LoginSuccessResponse { UserId = user.Id, Token = tokenResult.Value };
    }

    private async Task<ErrorOr<string>> GenerateOtpForMethodAsync(User user)
    {
        return user.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? await _securityProxyRepository.GenerateTotpAsync(user.Id)
            : await _securityProxyRepository.GenerateSmsOtpAsync(user.Id);
    }

    private async Task<ErrorOr<bool>> VerifyOtpForPreferredMethodAsync(User user, string code)
    {
        return user.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? await _securityProxyRepository.VerifyTotpAsync(user.Id, code)
            : await _securityProxyRepository.VerifySmsOtpAsync(user.Id, code);
    }

    private async Task<Error> HandleFailedPasswordAsync(User user)
    {
        _ = _authRepository.IncrementFailedAttempts(user.Id);
        int failedAttemptsAfterCurrentFailure = user.FailedLoginAttempts + 1;
        if (failedAttemptsAfterCurrentFailure < MaxFailedAttempts)
        {
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<Success> lockResult = _authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes));
        if (lockResult.IsError)
        {
            return AuthErrors.TooManyFailedAttempts;
        }

        ErrorOr<Success> emailResult = await _securityProxyRepository.SendLockNotificationAsync(user.Email);
        if (emailResult.IsError)
        {
            _logger.LogWarning(
                "Account lock email failed for user {UserId}: {Description}",
                user.Id,
                emailResult.FirstError.Description);
        }

        return AuthErrors.AccountLockedTooManyAttempts;
    }
}
