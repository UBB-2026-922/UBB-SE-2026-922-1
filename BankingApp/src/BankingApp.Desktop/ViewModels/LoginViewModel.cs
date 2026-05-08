namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.Features.Authentication.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Coordinates interactive sign-in for the desktop client.</summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
    public LoginViewModel(IAuthClientService authClientService, ILogger<LoginViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = _authClientService.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                _logger.LoginUnavailableApiClientNotConfigured(errors.Count);
                return LoginState.ServerNotConfigured;
            });
    }

    /// <summary>Gets or sets the current login workflow state.</summary>
    [ObservableProperty]
    public partial LoginState State { get; set; } = default!;

    /// <summary>Returns true when both email and password contain non-whitespace content.</summary>
    public static bool CanLogin(string email, string password) =>
        !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);

    /// <summary>Attempts to sign the user in with the supplied credentials.</summary>
    public async Task Login(string email, string password)
    {
        State = LoginState.Loading;
        ErrorOr<LoginSuccessResponse> result = await _authClientService.LoginAsync(email.Trim(), password);
        result.Switch(
            response =>
            {
                if (response.Requires2Fa)
                {
                    _authClientService.CurrentUserId = response.UserId;
                    State = LoginState.Require2Fa;
                    return;
                }

                _authClientService.SetToken(response.Token!);
                _authClientService.CurrentUserId = response.UserId;
                State = LoginState.Success;
            },
            errors =>
            {
                if (errors.First().Type == ErrorType.Forbidden)
                {
                    State = LoginState.AccountLocked;
                }
                else if (errors.First().Type == ErrorType.Unauthorized)
                {
                    State = LoginState.InvalidCredentials;
                }
                else
                {
                    _logger.LoginFailed(errors);
                    State = LoginState.Error;
                }
            });
    }
}
