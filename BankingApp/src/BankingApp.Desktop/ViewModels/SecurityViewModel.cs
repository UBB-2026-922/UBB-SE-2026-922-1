// <copyright file="SecurityViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the SecurityViewModel class.
// </summary>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Application.Enums;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Handles security-related profile operations such as password changes
///     and 2FA management.
/// </summary>
public class SecurityViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SecurityViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SecurityViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for security operations.</param>
    /// <param name="logger">Logger for security operation errors.</param>
    /// <returns>The result of the operation.</returns>
    public SecurityViewModel(IApiClient apiClient, ILogger<SecurityViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    /// <summary>
    ///     Gets the current security workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Enables or disables 2FA for the current user.
    /// </summary>
    /// <param name="enabled">
    ///     <see langword="true" /> to enable 2FA via email; <see langword="false" /> to disable it.
    /// </param>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    public async Task<bool> SetTwoFactorEnabled(bool enabled)
    {
        return enabled
            ? await EnableTwoFactor(TwoFactorMethod.Email)
            : await DisableTwoFactor();
    }

    /// <summary>
    ///     Changes the current user's password.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="currentPassword">The current password for verification.</param>
    /// <param name="newPassword">The new password to apply.</param>
    /// <param name="confirmPassword">The password confirmation.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(bool Success, string ErrorMessage)> ChangePassword(
        int userId,
        string currentPassword,
        string newPassword,
        string confirmPassword)
    {
        if (!PasswordValidator.MeetsMinimumLength(newPassword))
        {
            return (false, UserMessages.Security.MinimumLengthRequired);
        }

        if (newPassword != confirmPassword)
        {
            return (false, UserMessages.Security.PasswordMismatch);
        }

        State.SetValue(ProfileState.Loading);
        var request = new ChangePasswordRequest(userId, currentPassword, newPassword);
        ErrorOr<Success> result = await _apiClient.PutAsync(ApiEndpoints.ChangePassword, request);
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return (true, string.Empty);
            },
            errors =>
            {
                _logger.LogError("ChangePassword failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                string message = errors.First().Code == "incorrect_password"
                    ? UserMessages.Security.IncorrectPassword
                    : UserMessages.Security.UnexpectedError;
                return (false, message);
            });
    }

    /// <summary>
    ///     Enables 2FA for the current user.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true" /> if the setting was updated; otherwise, <see langword="false" />.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        State.SetValue(ProfileState.Loading);
        var request = new Enable2FaRequest { Method = method };
        ErrorOr<Success> result = await _apiClient.PutAsync(ApiEndpoints.Enable2Fa, request);
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.LogError("EnableTwoFactor failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    ///     Disables 2FA for the current user.
    /// </summary>
    /// <returns><see langword="true" /> if the setting was updated; otherwise, <see langword="false" />.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result = await _apiClient.PutAsync<object>(ApiEndpoints.Disable2Fa, new { });
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.LogError("DisableTwoFactor failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
