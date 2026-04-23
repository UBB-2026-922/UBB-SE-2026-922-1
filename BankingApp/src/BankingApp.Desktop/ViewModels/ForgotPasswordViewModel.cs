// <copyright file="ForgotPasswordViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ForgotPasswordViewModel class.
// </summary>

using System;
using System.Threading.Tasks;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Coordinates the forgot-password flow by delegating all business logic to
///     <see cref="IPasswordRecoveryManager" /> and exposing observable state to the View.
/// </summary>
public class ForgotPasswordViewModel
{
    private readonly IPasswordRecoveryManager _recoveryManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForgotPasswordViewModel" /> class
    ///     using a real <see cref="IApiClient" /> and the production system clock.
    /// </summary>
    /// <param name="apiClient">The HTTP client used for API calls.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiClient" /> is null.</exception>
    /// <returns>The result of the operation.</returns>
    public ForgotPasswordViewModel(IApiClient apiClient)
        : this(
            new PasswordRecoveryManager(
                apiClient ?? throw new ArgumentNullException(nameof(apiClient)),
                new SystemClock()))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForgotPasswordViewModel" /> class
    ///     with an explicit recovery manager (for testing).
    /// </summary>
    /// <param name="recoveryManager">The recovery manager to delegate to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recoveryManager" /> is null.</exception>
    /// <returns>The result of the operation.</returns>
    public ForgotPasswordViewModel(IPasswordRecoveryManager recoveryManager)
    {
        _recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));
        State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
    }

    /// <summary>
    ///     Gets the observable state of the forgot-password flow.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ForgotPasswordState> State { get; }

    /// <summary>
    ///     Gets a value indicating whether the user is currently allowed to resend a recovery code.
    /// </summary>
    /// <value>
    ///     A value indicating whether the user is currently allowed to resend a recovery code.
    /// </value>
    public bool CanResendCode => _recoveryManager.CanResendCode;

    /// <summary>
    ///     Gets the seconds remaining before the user may request another recovery code.
    /// </summary>
    /// <value>
    ///     The seconds remaining before the user may request another recovery code.
    /// </value>
    public int SecondsUntilResendAllowed => _recoveryManager.SecondsUntilResendAllowed;

    /// <summary>
    ///     Gets any pending validation error message.  Set before transitioning state so the
    ///     View can display it without needing its own validation logic.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ValidationError { get; private set; } = string.Empty;

    /// <summary>
    ///     Requests a password-recovery code for the given email address.
    /// </summary>
    /// <param name="email">The email address to send the code to.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ValidationError = UserMessages.ForgotPassword.EmailRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.RequestCodeAsync(email);
        State.SetValue(newState);
    }

    /// <summary>
    ///     Validates and resets the password using the supplied token.
    /// </summary>
    /// <param name="newPassword">The new password chosen by the user.</param>
    /// <param name="code">The reset token received by email.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ResetPassword(string newPassword, string code)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.AllFieldsRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        if (!_recoveryManager.IsPasswordValid(newPassword))
        {
            ValidationError = UserMessages.ForgotPassword.PasswordTooWeak;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.ResetPasswordAsync(code, newPassword);
        State.SetValue(newState);
    }

    /// <summary>
    ///     Verifies whether the supplied reset token is still valid.
    /// </summary>
    /// <param name="code">The token to check.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task VerifyToken(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ValidationError = UserMessages.ForgotPassword.CodeRequired;
            State.SetValue(ForgotPasswordState.Error);
            return;
        }

        ValidationError = string.Empty;
        ForgotPasswordState newState = await _recoveryManager.VerifyTokenAsync(code);
        State.SetValue(newState);
    }
}