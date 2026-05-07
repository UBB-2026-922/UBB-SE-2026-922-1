using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Auth;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class LoginViewModel
{
    private readonly IAuthClientService _authClientService;
    private readonly ILogger<LoginViewModel> _logger;

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

    public ObservableState<LoginState> State { get; }

    public static bool CanLogin(string email, string password)
    {
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

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
