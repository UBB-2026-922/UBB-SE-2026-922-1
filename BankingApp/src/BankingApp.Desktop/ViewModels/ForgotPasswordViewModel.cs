namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Features.PasswordRecovery;
using Shared;
using Shared.Enums;

/// <summary>Coordinates the desktop forgot-password flow through <see cref="IPasswordRecoveryService"/>.</summary>
public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IPasswordRecoveryService _passwordRecoveryService;

    /// <summary>Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class.</summary>
    public ForgotPasswordViewModel(IPasswordRecoveryService passwordRecoveryService)
    {
        _passwordRecoveryService = passwordRecoveryService ?? throw new ArgumentNullException(nameof(passwordRecoveryService));
    }

    /// <summary>Gets or sets the current forgot-password workflow state.</summary>
    [ObservableProperty]
    public partial ForgotPasswordState State { get; set; } = ForgotPasswordState.Idle;

    /// <summary>Gets or sets the latest client-side validation error message.</summary>
    [ObservableProperty]
    public partial string ValidationError { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether a new reset code may be requested.</summary>
    public bool CanResendCode => _passwordRecoveryService.CanResendCode;

    /// <summary>Gets the remaining seconds until another code can be requested.</summary>
    public int SecondsUntilResendAllowed => _passwordRecoveryService.SecondsUntilResendAllowed;

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
        State = await _passwordRecoveryService.RequestCodeAsync(email);
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

        if (!_passwordRecoveryService.IsPasswordValid(newPassword))
        {
            ValidationError = UserMessages.ForgotPassword.PasswordTooWeak;
            State = ForgotPasswordState.Error;
            return;
        }

        ValidationError = string.Empty;
        State = await _passwordRecoveryService.ResetPasswordAsync(code, newPassword);
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
        State = await _passwordRecoveryService.VerifyTokenAsync(code);
    }
}
