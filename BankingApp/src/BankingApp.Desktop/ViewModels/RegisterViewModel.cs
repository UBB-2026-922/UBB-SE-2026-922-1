// <copyright file="RegisterViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
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
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Coordinates registration requests for the register view.
/// </summary>
public class RegisterViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<RegisterViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for registration requests.</param>
    /// <param name="logger">Logger for registration flow errors.</param>
    /// <returns>The result of the operation.</returns>
    public RegisterViewModel(IApiClient apiClient, ILogger<RegisterViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
            FullName = fullName
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

    private static RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
            return RegisterState.Error;

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
            return RegisterState.InvalidEmail;

        if (!PasswordValidator.IsStrong(password)) return RegisterState.WeakPassword;

        if (password != confirmPassword) return RegisterState.PasswordMismatch;

        return null;
    }
}
