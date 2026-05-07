using System;
using System.Threading.Tasks;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;

namespace BankingApp.Desktop.ViewModels;

public partial class ForgotPasswordViewModel
{
    private readonly IPasswordRecoveryManager _recoveryManager;

    public ForgotPasswordViewModel(IPasswordRecoveryManager recoveryManager)
    {
        _recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));
        State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
    }

    public ObservableState<ForgotPasswordState> State { get; }

    public bool CanResendCode => _recoveryManager.CanResendCode;

    public int SecondsUntilResendAllowed => _recoveryManager.SecondsUntilResendAllowed;

    public string ValidationError { get; private set; } = string.Empty;

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
