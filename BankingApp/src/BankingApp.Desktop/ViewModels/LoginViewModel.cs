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

/// <summary>
/// Coordinates interactive sign-in for the desktop client.
/// </summary>
public class LoginViewModel
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authClientService">Provides login and session configuration operations.</param>
    /// <param name="logger">Writes operational diagnostics for sign-in failures.</param>
    public LoginViewModel(IAuthClientService authClientService, ILogger<LoginViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoginState initialState = _authClientService.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                _logger.LoginUnavailableApiClientNotConfigured(errors.Count);
                return LoginState.ServerNotConfigured;
            });
        State = new ObservableState<LoginState>(initialState);
    }

    /// <summary>
    /// Gets the observable state of the current login flow.
    /// </summary>
    public ObservableState<LoginState> State { get; }

    /// <summary>
    /// Determines whether the provided credentials are sufficient to submit a login request.
    /// </summary>
    /// <param name="email">The email entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns><see langword="true"/> when both inputs contain non-whitespace content; otherwise, <see langword="false"/>.</returns>
    public static bool CanLogin(string email, string password)
    {
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

    /// <summary>
    /// Attempts to sign the user in with the supplied credentials.
    /// </summary>
    /// <param name="email">The email entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns>A task that completes when the login attempt finishes.</returns>
    public async Task Login(string email, string password)
    {
        State.SetValue(LoginState.Loading);
        ErrorOr<LoginSuccessResponse> result = await _authClientService.LoginAsync(email.Trim(), password);
        result.Switch(
            response =>
            {
                if (response.Requires2Fa)
                {
                    _authClientService.CurrentUserId = response.UserId;
                    State.SetValue(LoginState.Require2Fa);
                    return;
                }

                _authClientService.SetToken(response.Token!);
                _authClientService.CurrentUserId = response.UserId;
                State.SetValue(LoginState.Success);
            },
            errors =>
            {
                if (errors.First().Type == ErrorType.Forbidden)
                {
                    State.SetValue(LoginState.AccountLocked);
                }
                else if (errors.First().Type == ErrorType.Unauthorized)
                {
                    State.SetValue(LoginState.InvalidCredentials);
                }
                else
                {
                    _logger.LoginFailed(errors);
                    State.SetValue(LoginState.Error);
                }
            });
    }
}
