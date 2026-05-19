namespace BankingApp.Desktop.Features.PasswordRecovery;

using Application.Shared.Clock;
using Application.Features.Authentication.Services;
using Contracts.Features.PasswordReset.Dtos;
using ErrorOr;
using Shared.Enums;
using Shared.Validation;

/// <summary>Default desktop password recovery flow service.</summary>
public sealed class PasswordRecoveryService(IAuthenticationService authenticationService, ISystemClock clock)
    : IPasswordRecoveryService
{
    private const int ResendCooldownSeconds = 60;
    private DateTime? _lastCodeRequestedAt;

    /// <inheritdoc />
    public bool CanResendCode =>
        _lastCodeRequestedAt is null || (clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;

    /// <inheritdoc />
    public int SecondsUntilResendAllowed
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return 0;
            }

            double remaining = ResendCooldownSeconds - (clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds;
            return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
        }
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> RequestCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ForgotPasswordState.Error;
        }

        if (!CanResendCode)
        {
            return ForgotPasswordState.EmailSent;
        }

        ErrorOr<Success> result = await authenticationService.ForgotPasswordAsync(
            new ForgotPasswordRequest { Email = email },
            cancellationToken);

        if (result.IsError)
        {
            return ForgotPasswordState.Error;
        }

        _lastCodeRequestedAt = clock.UtcNow;
        return ForgotPasswordState.EmailSent;
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> VerifyTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await authenticationService.VerifyResetTokenAsync(
            new VerifyResetTokenRequest { Token = token },
            cancellationToken);

        return result.IsError ? MapError(result.FirstError) : ForgotPasswordState.TokenValid;
    }

    /// <inheritdoc />
    public async Task<ForgotPasswordState> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await authenticationService.ResetPasswordAsync(
            new ResetPasswordRequest
            {
                Token = token,
                NewPassword = newPassword,
            },
            cancellationToken);

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
