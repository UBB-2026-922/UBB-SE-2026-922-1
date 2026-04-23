// <copyright file="LoginViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the LoginViewModel class.
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
using LoginRequest = BankingApp.Application.DataTransferObjects.Auth.LoginRequest;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Coordinates credential-based and OAuth login requests for the login view.
/// </summary>
public class LoginViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for authentication requests.</param>
    /// <param name="configuration">
    ///     The application _configuration. Reads <c>OAuth:Google:Authority</c>,
    ///     <c>OAuth:Google:ClientId</c>, <c>OAuth:Google:ClientSecret</c>, and
    ///     <c>OAuth:Google:RedirectUri</c> when performing an OAuth login.
    /// </param>
    /// <param name="logger">Logger for login flow diagnostics and errors.</param>
    /// <returns>The result of the operation.</returns>
    public LoginViewModel(IApiClient apiClient, IConfiguration configuration, ILogger<LoginViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Determine initial state from _configuration. If the API client is misconfigured the
        // view starts in ServerNotConfigured so the login form is disabled immediately.
        // The view reads State.Value after subscribing to apply this initial state.
        LoginState initialState = _apiClient.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                _logger.LogCritical("ApiClient is not configured — login is unavailable. Errors: {Errors}", errors);
                return LoginState.ServerNotConfigured;
            });
        State = new ObservableState<LoginState>(initialState);
    }

    /// <summary>
    ///     Gets the current login flow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<LoginState> State { get; }

    /// <summary>
    ///     Returns <see langword="true" /> when both <paramref name="email" /> and
    ///     <paramref name="password" /> are non-empty and a login attempt can be made.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns><see langword="true" /> if the inputs are sufficient to attempt login.</returns>
    public bool CanLogin(string email, string password)
    {
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

    /// <summary>
    ///     Attempts to sign in with the provided email address and password.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task Login(string email, string password)
    {
        State.SetValue(LoginState.Loading);
        var request = new LoginRequest
        {
            Email = email.Trim(),
            Password = password,
        };
        ErrorOr<LoginSuccessResponse> result = await _apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(
            ApiEndpoints.Login,
            request);
        result.Switch(
            response =>
            {
                if (response.Requires2Fa)
                {
                    _apiClient.CurrentUserId = response.UserId;
                    State.SetValue(LoginState.Require2Fa);
                    return;
                }

                _apiClient.SetToken(response.Token!);
                _apiClient.CurrentUserId = response.UserId;
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
                    _logger.LogError("Login failed: {Errors}", errors);
                    State.SetValue(LoginState.Error);
                }
            });
    }

    /// <summary>
    ///     Attempts to sign in with the specified OAuth provider.
    /// </summary>
    /// <param name="provider">The OAuth provider to authenticate against.</param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    public async Task OAuthLogin(string provider)
    {
        State.SetValue(LoginState.Loading);
        try
        {
            if (!provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                State.SetValue(LoginState.Error);
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
            LoginResult loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());
            if (loginResult.IsError)
            {
                State.SetValue(LoginState.Error);
                return;
            }

            var apiRequest = new OAuthLoginRequest
            {
                Provider = "Google",
                ProviderToken = loginResult.IdentityToken,
            };
            ErrorOr<LoginSuccessResponse> result = await _apiClient.PostAsync<OAuthLoginRequest, LoginSuccessResponse>(
                ApiEndpoints.OAuthLogin,
                apiRequest);
            result.Switch(
                response =>
                {
                    if (response.Requires2Fa)
                    {
                        _apiClient.CurrentUserId = response.UserId;
                        State.SetValue(LoginState.Require2Fa);
                        return;
                    }

                    _apiClient.SetToken(response.Token!);
                    _apiClient.CurrentUserId = response.UserId;
                    State.SetValue(LoginState.Success);
                },
                errors =>
                {
                    _logger.LogError("OAuthLogin failed: {Errors}", errors);
                    State.SetValue(LoginState.Error);
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "OAuthLogin OIDC flow failed.");
            State.SetValue(LoginState.Error);
        }
    }
}