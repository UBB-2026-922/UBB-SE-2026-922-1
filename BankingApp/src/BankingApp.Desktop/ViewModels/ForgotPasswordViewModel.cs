namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Enums;
using Utilities;

/// <summary>Coordinates the desktop forgot-password flow through <see cref="IPasswordRecoveryManager"/>.</summary>
public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IPasswordRecoveryManager _recoveryManager;

    /// <summary>Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class.</summary>
    public ForgotPasswordViewModel(IPasswordRecoveryManager recoveryManager)
    {
        _recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));
    }

    /// <summary>Gets or sets the current forgot-password workflow state.</summary>
    [ObservableProperty]
    public partial ForgotPasswordState State { get; set; } = ForgotPasswordState.Idle;

    /// <summary>Gets or sets the latest client-side validation error message.</summary>
    [ObservableProperty]
    public partial string ValidationError { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether a new reset code may be requested.</summary>
    public bool CanResendCode => _recoveryManager.CanResendCode;

    /// <summary>Gets the remaining seconds until another code can be requested.</summary>
    public int SecondsUntilResendAllowed => _recoveryManager.SecondsUntilResendAllowed;

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
        State = await _recoveryManager.RequestCodeAsync(email);
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

        if (!_recoveryManager.IsPasswordValid(newPassword))
        {
            ValidationError = UserMessages.ForgotPassword.PasswordTooWeak;
            State = ForgotPasswordState.Error;
            return;
        }

        ValidationError = string.Empty;
        State = await _recoveryManager.ResetPasswordAsync(code, newPassword);
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
        State = await _recoveryManager.VerifyTokenAsync(code);
    }
}
