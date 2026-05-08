namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Coordinates user-registration requests for the register view.</summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<RegisterViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="RegisterViewModel"/> class.</summary>
    public RegisterViewModel(IAuthClientService authClientService, ILogger<RegisterViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets or sets the current registration workflow state.</summary>
    [ObservableProperty]
    public partial RegisterState State { get; set; } = RegisterState.Idle;

    /// <summary>Registers a new account using the supplied email, password, and full name.</summary>
    public async Task Register(string email, string password, string confirmPassword, string fullName)
    {
        email = email.Trim();
        fullName = fullName.Trim();
        RegisterState? validationError = ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            State = validationError.Value;
            return;
        }

        State = RegisterState.Loading;
        ErrorOr<Success> result = await _authClientService.RegisterAsync(email, password, fullName);
        result.Switch(
            _ => { State = RegisterState.Success; },
            errors =>
            {
                Error error = errors.First();
                if (error.Type == ErrorType.Conflict)
                {
                    State = RegisterState.EmailAlreadyExists;
                }
                else if (error.Code == "invalid_email")
                {
                    State = RegisterState.InvalidEmail;
                }
                else if (error.Code == "weak_password")
                {
                    State = RegisterState.WeakPassword;
                }
                else
                {
                    _logger.RegisterFailed(errors);
                    State = RegisterState.Error;
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
