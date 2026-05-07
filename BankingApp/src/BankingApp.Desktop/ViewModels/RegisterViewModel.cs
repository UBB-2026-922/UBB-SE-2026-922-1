using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class RegisterViewModel
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<RegisterViewModel> _logger;

    public RegisterViewModel(IAuthClientService authClientService, ILogger<RegisterViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<RegisterState>(RegisterState.Idle);
    }

    public ObservableState<RegisterState> State { get; }

    public async Task Register(string email, string password, string confirmPassword, string fullName)
    {
        email = email?.Trim() ?? string.Empty;
        fullName = fullName?.Trim() ?? string.Empty;
        RegisterState? validationError = ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            State.SetValue(validationError.Value);
            return;
        }

        State.SetValue(RegisterState.Loading);
        ErrorOr<Success> result = await _authClientService.RegisterAsync(email, password, fullName);
        result.Switch(
            _ => { State.SetValue(RegisterState.Success); },
            errors =>
            {
                Error error = errors.First();
                if (error.Type == ErrorType.Conflict)
                {
                    State.SetValue(RegisterState.EmailAlreadyExists);
                }
                else if (error.Code == "invalid_email")
                {
                    State.SetValue(RegisterState.InvalidEmail);
                }
                else if (error.Code == "weak_password")
                {
                    State.SetValue(RegisterState.WeakPassword);
                }
                else
                {
                    _logger.RegisterFailed(errors);
                    State.SetValue(RegisterState.Error);
                }
            });
    }

    private static RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
            return RegisterState.Error;

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
            return RegisterState.InvalidEmail;

        if (!PasswordValidator.IsStrong(password)) return RegisterState.WeakPassword;

        if (password != confirmPassword) return RegisterState.PasswordMismatch;

        return null;
    }
}
