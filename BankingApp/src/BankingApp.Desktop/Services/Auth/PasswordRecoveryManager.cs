namespace BankingApp.Desktop.Utilities;

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.Features.PasswordReset.Dtos;
using Enums;
using ErrorOr;

/// <summary>
///     Implements <see cref="IPasswordRecoveryManager" /> by delegating network calls
///     to <see cref="IApiClient" /> and managing resend-throttling via <see cref="ISystemClock" />.
/// </summary>
public class PasswordRecoveryManager : IPasswordRecoveryManager
{
    private const int ResendCooldownSeconds = 60;
    private const int NoSecondsRemaining = 0;
    private readonly IApiClient _apiClient;
    private readonly ISystemClock _clock;
    private DateTime? _lastCodeRequestedAt;

    /// <summary>Initializes a new instance of the <see cref="PasswordRecoveryManager" /> class.</summary>
    public PasswordRecoveryManager(IApiClient apiClient, ISystemClock clock)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <inheritdoc />
    public bool CanResendCode
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return true;
            }

            return (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;
        }
    }

    /// <inheritdoc />
    public int SecondsUntilResendAllowed
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return NoSecondsRemaining;
            }

            double elapsed = (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds;
            double remaining = ResendCooldownSeconds - elapsed;
            return remaining > 0 ? (int)Math.Ceiling(remaining) : NoSecondsRemaining;
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

        var request = new ForgotPasswordRequest { Email = email };
        ErrorOr<ApiResponse> result = await _apiClient.PostAsync<ForgotPasswordRequest, ApiResponse>(
            ApiEndpoints.ForgotPassword,
            request);
        return result.Match(
            response =>
            {
                if (response.Error == null)
                {
                    _lastCodeRequestedAt = _clock.UtcNow;
                    return ForgotPasswordState.EmailSent;
                }

                return ForgotPasswordState.Error;
            },
            _ => ForgotPasswordState.Error);
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.VerifyResetToken, new { Token = token });
        return result.Match(
            _ => ForgotPasswordState.TokenValid,
            errors => MapError(errors.First()));
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ForgotPasswordState.Error;
        }

        var request = new ResetPasswordRequest { Token = token, NewPassword = newPassword };
        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.ResetPassword, request);
        return result.Match(
            _ => ForgotPasswordState.PasswordResetSuccess,
            errors => MapError(errors.First()));
    }

    /// <inheritdoc />
    public bool IsPasswordValid(string password) => PasswordValidator.IsStrong(password);

    private static ForgotPasswordState MapError(Error error)
    {
        return error.Code switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            _ => ForgotPasswordState.Error
        };
    }
}
