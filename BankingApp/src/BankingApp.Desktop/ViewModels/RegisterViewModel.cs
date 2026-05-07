namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Enums;
using Services;
using Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Coordinates user-registration requests for the register view.
/// </summary>
public class RegisterViewModel
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<RegisterViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterViewModel" /> class.
    /// </summary>
    public RegisterViewModel(IAuthClientService authClientService, ILogger<RegisterViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<RegisterState>(RegisterState.Idle);
    }

    /// <summary>
    ///     Gets the current registration workflow state.
    /// </summary>
    public ObservableState<RegisterState> State { get; }

    /// <summary>
    ///     Registers a new account using the supplied email, password, and full name.
    /// </summary>
    public async Task Register(string email, string password, string confirmPassword, string fullName)
    {
        email = email.Trim();
        fullName = fullName.Trim();
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
        {
            return RegisterState.Error;
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            return RegisterState.InvalidEmail;
        }

        if (!PasswordValidator.IsStrong(password))
        {
            return RegisterState.WeakPassword;
        }

        if (password != confirmPassword)
        {
            return RegisterState.PasswordMismatch;
        }

        return null;
    }
}
