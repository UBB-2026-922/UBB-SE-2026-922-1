namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Authentication.Dtos;
using Enums;
using Services;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>Coordinates interactive sign-in for the desktop client.</summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthClientService _authClientService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
    public LoginViewModel(
        IAuthClientService authClientService,
        IConfiguration configuration,
        ILogger<LoginViewModel> logger)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        IsDevLoginAvailable = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);
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
        ErrorOr<LoginSuccessResponse> result = await _authClientService.LoginAsync(email.Trim(), password);
        if (result.IsError)
        {
            _logger.LoginFailed(result.Errors);
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

        _authClientService.SetToken(response.Token);
        _authClientService.CurrentUserId = response.UserId;
        State = LoginState.Success;
        return Result.Success;
    }

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
