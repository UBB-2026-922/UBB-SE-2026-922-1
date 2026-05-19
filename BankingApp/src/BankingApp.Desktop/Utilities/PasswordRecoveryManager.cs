namespace BankingApp.Desktop.Utilities;

using System;
using System.Threading.Tasks;
using Application.Common.Http;
using BankingApp.Desktop.Enums;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Http;
using ErrorOr;

/// <summary>Default desktop implementation of <see cref="IPasswordRecoveryManager"/>.</summary>
public sealed class PasswordRecoveryManager : IPasswordRecoveryManager
{
    private const int ResendCooldownSeconds = 60;
    private readonly IApiClient _apiClient;
    private readonly ISystemClock _clock;
    private DateTime? _lastCodeRequestedAt;

    /// <summary>Initializes a new instance of the <see cref="PasswordRecoveryManager"/> class.</summary>
    public PasswordRecoveryManager(IApiClient apiClient, ISystemClock clock)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <inheritdoc />
    public bool CanResendCode =>
        _lastCodeRequestedAt is null || (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;

    /// <inheritdoc />
    public int SecondsUntilResendAllowed
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return 0;
            }

            double remaining = ResendCooldownSeconds - (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds;
            return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
        }
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> RequestCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ForgotPasswordState.Error;
        }

        if (!CanResendCode)
        {
            return ForgotPasswordState.EmailSent;
        }

        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.ForgotPassword, new ForgotPasswordRequest { Email = email });
        if (result.IsError)
        {
            return ForgotPasswordState.Error;
        }

        _lastCodeRequestedAt = _clock.UtcNow;
        return ForgotPasswordState.EmailSent;
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await _apiClient.PostAsync<object>(ApiEndpoints.VerifyResetToken, new { Token = token });
        return result.IsError ? MapError(result.FirstError) : ForgotPasswordState.TokenValid;
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.ResetPassword, new ResetPasswordRequest
        {
            Token = token,
            NewPassword = newPassword,
        });

        return result.IsError ? MapError(result.FirstError) : ForgotPasswordState.PasswordResetSuccess;
    }

    /// <inheritdoc />
    public bool IsPasswordValid(string password) => PasswordValidator.IsStrong(password);

    private static ForgotPasswordState MapError(Error error) =>
        error.Code switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            _ => ForgotPasswordState.Error,
        };
}
