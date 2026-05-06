// <copyright file="LoginViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the LoginViewModel class.
// </summary>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

using Application.DTOs.Auth;

/// <summary>
///     Coordinates credential-based login requests for the login view.
/// </summary>
public partial class LoginViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<LoginViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for authentication requests.</param>
    /// <param name="logger">Logger for login flow diagnostics and errors.</param>
    /// <returns>The result of the operation.</returns>
    public LoginViewModel(IApiClient apiClient, ILogger<LoginViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // Determine initial state from _configuration. If the API client is misconfigured the
        // view starts in ServerNotConfigured so the login form is disabled immediately.
        // The view reads State.Value after subscribing to apply this initial state.
        LoginState initialState = _apiClient.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                _logger.LoginUnavailableApiClientNotConfigured(errors.Count);
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
    public static bool CanLogin(string email, string password)
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
            Password = password
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
                    _logger.LoginFailed(errors);
                    State.SetValue(LoginState.Error);
                }
            });
    }
}
