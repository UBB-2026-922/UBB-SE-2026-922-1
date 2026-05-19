namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Authentication.Services;
using BankingApp.Contracts.Features.Authentication.Dtos;
using ErrorOr;
using Features.Authentication;
using Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Coordinates interactive sign-in for the desktop client.</summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthenticationSession _authenticationSession;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
    public LoginViewModel(
        IAuthenticationService authenticationService,
        IAuthenticationSession authenticationSession,
        IConfiguration configuration,
        ILogger<LoginViewModel> logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _authenticationSession = authenticationSession ?? throw new ArgumentNullException(nameof(authenticationSession));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        IsDevLoginAvailable = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = _authenticationSession.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                DesktopLogMessages.LoginUnavailableApiClientNotConfigured(_logger, errors.Count);
                return LoginState.ServerNotConfigured;
            });
    }

    /// <summary>Gets or sets the current login workflow state.</summary>
    [ObservableProperty]
    public partial LoginState State { get; set; } = default!;

    /// <summary>Gets a value indicating whether fast development login is available.</summary>
    public bool IsDevLoginAvailable { get; }

    /// <summary>Returns true when both email and password contain non-whitespace content.</summary>
    public static bool CanLogin(string email, string password) =>
        !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);

    /// <summary>Attempts to sign the configured development user in without manual entry.</summary>
    public async Task<ErrorOr<Success>> DevLogin()
    {
        string? email = _configuration["DevLogin:Email"];
        string? password = _configuration["DevLogin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Error.Failure(
                "DevLogin.NotConfigured",
                "Dev login is not configured. Set DevLogin:Email and DevLogin:Password in appsettings.Development.json.");
        }

        State = LoginState.Loading;
        ErrorOr<LoginSuccessResponse> result = await _authenticationService.LoginAsync(
            new LoginRequest { Email = email.Trim(), Password = password });
        if (result.IsError)
        {
            DesktopLogMessages.LoginFailed(_logger, result.Errors);
            State = LoginState.Idle;
            return result.Errors;
        }

        LoginSuccessResponse response = result.Value;
        if (response.Requires2Fa)
        {
            State = LoginState.Idle;
            return Error.Failure(
                "DevLogin.Requires2Fa",
                "Dev login cannot use an account that requires two-factor authentication.");
        }

        if (string.IsNullOrWhiteSpace(response.Token))
        {
            State = LoginState.Idle;
            return Error.Failure(
                "DevLogin.MissingToken",
                "Dev login failed because the API did not return an authentication token.");
        }

        _authenticationSession.SetToken(response.Token);
        _authenticationSession.CurrentUserId = response.UserId;
        State = LoginState.Success;
        return Result.Success;
    }

    /// <summary>Attempts to sign the user in with the supplied credentials.</summary>
    public async Task Login(string email, string password)
    {
        State = LoginState.Loading;
        ErrorOr<LoginSuccessResponse> result = await _authenticationService.LoginAsync(
            new LoginRequest { Email = email.Trim(), Password = password });
        result.Switch(
            response =>
            {
                if (response.Requires2Fa)
                {
                    _authenticationSession.CurrentUserId = response.UserId;
                    State = LoginState.Require2Fa;
                    return;
                }

                _authenticationSession.SetToken(response.Token!);
                _authenticationSession.CurrentUserId = response.UserId;
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
                    DesktopLogMessages.LoginFailed(_logger, errors);
                    State = LoginState.Error;
                }
            });
    }
}
