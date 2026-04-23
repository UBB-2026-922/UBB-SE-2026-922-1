// <copyright file="RegisterViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RegisterViewModel class.
// </summary>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using Duende.IdentityModel.OidcClient;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LoginRequest = Duende.IdentityModel.OidcClient.LoginRequest;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Coordinates registration requests for the register view.
/// </summary>
public class RegisterViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for registration requests.</param>
    /// <param name="configuration">
    ///     The application _configuration. Reads <c>OAuth:Google:Authority</c>,
    ///     <c>OAuth:Google:ClientId</c>, <c>OAuth:Google:ClientSecret</c>, and
    ///     <c>OAuth:Google:RedirectUri</c> when performing an OAuth registration.
    /// </param>
    /// <param name="logger">Logger for registration flow errors.</param>
    /// <returns>The result of the operation.</returns>
    public RegisterViewModel(IApiClient apiClient, IConfiguration configuration, ILogger<RegisterViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<RegisterState>(RegisterState.Idle);
    }

    /// <summary>
    ///     Gets the current state of the registration flow.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<RegisterState> State { get; }

    /// <summary>
    ///     Registers a new account using email and password credentials.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <param name="confirmPassword">The confirmation password entered by the user.</param>
    /// <param name="fullName">The full name entered by the user.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
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
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            FullName = fullName,
        };
        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.Register, request);
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
                    _logger.LogError("Register failed: {Errors}", errors);
                    State.SetValue(RegisterState.Error);
                }
            });
    }

    /// <summary>
    ///     Registers or signs in a user through the specified OAuth provider.
    /// </summary>
    /// <param name="provider">The OAuth provider to use.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task OAuthRegister(string provider)
    {
        State.SetValue(RegisterState.Loading);
        try
        {
            if (!provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                State.SetValue(RegisterState.Error);
                return;
            }

            string authority = _configuration["OAuth:Google:Authority"]
                               ?? throw new InvalidOperationException(
                                   "OAuth:Google:Authority is missing from _configuration.");
            string clientId = _configuration["OAuth:Google:ClientId"]
                              ?? throw new InvalidOperationException(
                                  "OAuth:Google:ClientId is missing from _configuration.");
            string clientSecret = _configuration["OAuth:Google:ClientSecret"]
                                  ?? throw new InvalidOperationException(
                                      "OAuth:Google:ClientSecret is missing from _configuration.");
            string redirectUri = _configuration["OAuth:Google:RedirectUri"]
                                 ?? throw new InvalidOperationException(
                                     "OAuth:Google:RedirectUri is missing from _configuration.");
            var options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "openid email profile",
                RedirectUri = redirectUri,
                Browser = new SystemBrowser(new Uri(redirectUri).Port),
            };
            options.Policy.Discovery.ValidateEndpoints = false;
            var oidcClient = new OidcClient(options);
            LoginResult loginResult = await oidcClient.LoginAsync(new LoginRequest());
            if (loginResult.IsError)
            {
                State.SetValue(RegisterState.Error);
                return;
            }

            var apiRequest = new OAuthLoginRequest
            {
                Provider = "Google",
                ProviderToken = loginResult.IdentityToken,
            };
            ErrorOr<LoginSuccessResponse> result =
                await _apiClient.PostAsync<OAuthLoginRequest, LoginSuccessResponse>(
                    ApiEndpoints.OAuthLogin,
                    apiRequest);
            result.Switch(
                response =>
                {
                    _apiClient.SetToken(response.Token!);
                    _apiClient.CurrentUserId = response.UserId;
                    State.SetValue(RegisterState.AutoLoggedIn);
                },
                errors =>
                {
                    _logger.LogError("OAuthRegister failed: {Errors}", errors);
                    State.SetValue(RegisterState.Error);
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "OAuthRegister OIDC flow failed.");
            State.SetValue(RegisterState.Error);
        }
    }

    private RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return RegisterState.Error;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@", StringComparison.Ordinal))
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