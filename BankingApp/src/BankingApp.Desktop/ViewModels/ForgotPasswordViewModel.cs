namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Application.Features.Authentication.Services;
using Application.Shared.Clock;
using Contracts.Features.PasswordReset.Dtos;
using ErrorOr;
using Shared;
using Shared.Enums;
using Shared.Validation;

/// <summary>Coordinates the desktop forgot-password flow.</summary>
public partial class ForgotPasswordViewModel : ObservableObject
{
    private const int ResendCooldownSeconds = 60;
    private readonly IAuthenticationService _authenticationService;
    private readonly ISystemClock _clock;
    private DateTime? _lastCodeRequestedAt;

    /// <summary>Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class.</summary>
    public ForgotPasswordViewModel(IAuthenticationService authenticationService, ISystemClock clock)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Gets or sets the current forgot-password workflow state.</summary>
    [ObservableProperty]
    public partial ForgotPasswordState State { get; set; } = ForgotPasswordState.Idle;

    /// <summary>Gets or sets the latest client-side validation error message.</summary>
    [ObservableProperty]
    public partial string ValidationError { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether a new reset code may be requested.</summary>
    public bool CanResendCode =>
        _lastCodeRequestedAt is null || (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;

    /// <summary>Gets the remaining seconds until another code can be requested.</summary>
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

    /// <summary>Starts the password-reset flow for the supplied email address.</summary>
    public async Task ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ValidationError = UserMessages.ForgotPassword.EmailRequired;
            State = ForgotPasswordState.Error;
            return;
        }

        ValidationError = string.Empty;
        if (!CanResendCode)
        {
            State = ForgotPasswordState.EmailSent;
            return;
        }

        ErrorOr<Success> result = await _authenticationService.ForgotPasswordAsync(
            new ForgotPasswordRequest { Email = email });

        if (result.IsError)
        {
            State = ForgotPasswordState.Error;
            return;
        }

        _lastCodeRequestedAt = _clock.UtcNow;
        State = ForgotPasswordState.EmailSent;
    }

    /// <summary>Resets the password with the supplied code and new password.</summary>
    public async Task ResetPassword(string newPassword, string code)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.AllFieldsRequired;
            State = ForgotPasswordState.Error;
            return;
        }

        if (!PasswordValidator.IsStrong(newPassword))
        {
            ValidationError = UserMessages.ForgotPassword.PasswordTooWeak;
            State = ForgotPasswordState.Error;
            return;
        }

        ValidationError = string.Empty;
        ErrorOr<Success> result = await _authenticationService.ResetPasswordAsync(
            new ResetPasswordRequest
            {
                Token = code,
                NewPassword = newPassword,
            });

        State = result.IsError ? MapError(result.FirstError) : ForgotPasswordState.PasswordResetSuccess;
    }

    /// <summary>Verifies whether the supplied reset token is still valid.</summary>
    public async Task VerifyToken(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.CodeRequired;
            State = ForgotPasswordState.Error;
            return;
        }

        ValidationError = string.Empty;
        ErrorOr<Success> result = await _authenticationService.VerifyResetTokenAsync(
            new VerifyResetTokenRequest { Token = code });

        State = result.IsError ? MapError(result.FirstError) : ForgotPasswordState.TokenValid;
    }

    private static ForgotPasswordState MapError(Error error) =>
        error.Code switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            _ => ForgotPasswordState.Error,
        };
}
