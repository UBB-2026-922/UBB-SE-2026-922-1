namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Enums;
using Utilities;

/// <summary>
///     Coordinates the desktop forgot-password flow through <see cref="IPasswordRecoveryManager" />.
/// </summary>
public class ForgotPasswordViewModel
{
    private readonly IPasswordRecoveryManager _recoveryManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForgotPasswordViewModel" /> class.
    /// </summary>
    public ForgotPasswordViewModel(IPasswordRecoveryManager recoveryManager)
    {
        _recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));
        State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
    }

    /// <summary>
    ///     Gets the current forgot-password workflow state.
    /// </summary>
    public ObservableState<ForgotPasswordState> State { get; }

    /// <summary>
    ///     Gets a value indicating whether a new reset code may be requested.
    /// </summary>
    public bool CanResendCode => _recoveryManager.CanResendCode;

    /// <summary>
    ///     Gets the remaining seconds until another code can be requested.
    /// </summary>
    public int SecondsUntilResendAllowed => _recoveryManager.SecondsUntilResendAllowed;

    /// <summary>
    ///     Gets the latest client-side validation error.
    /// </summary>
    public string ValidationError { get; private set; } = string.Empty;

    /// <summary>
    ///     Starts the password-reset flow for the supplied email address.
    /// </summary>
    public async Task ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ValidationError = UserMessages.ForgotPassword.EmailRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.RequestCodeAsync(email);
        State.SetValue(newState);
    }

    /// <summary>
    ///     Resets the password with the supplied code and new password.
    /// </summary>
    public async Task ResetPassword(string newPassword, string code)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.AllFieldsRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        if (!_recoveryManager.IsPasswordValid(newPassword))
        {
            ValidationError = UserMessages.ForgotPassword.PasswordTooWeak;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.ResetPasswordAsync(code, newPassword);
        State.SetValue(newState);
    }

    /// <summary>
    ///     Verifies whether the supplied reset token is still valid.
    /// </summary>
    public async Task VerifyToken(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.CodeRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.VerifyTokenAsync(code);
        State.SetValue(newState);
    }
}
