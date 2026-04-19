// <copyright file="TwoFactorViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TwoFactorViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
///     Coordinates OTP verification and resend operations for the 2FA flow.
///     Owns the resend-countdown state and exposes observable properties that the view
///     binds to directly via <c>{x:Bind}</c>, keeping all business decisions out of the
///     code-behind.
/// </summary>
public partial class TwoFactorViewModel : INotifyPropertyChanged
{
    private const int ResendCooldownSeconds = 30;
    private const int OtpRequiredLength = 6;
    private readonly IApiClient _apiClient;
    private readonly ICountdownTimer _countdownTimer;
    private readonly ILogger<TwoFactorViewModel> _logger;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isLoading;
    private string _otpCode = string.Empty;
    private int _secondsRemaining;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TwoFactorViewModel" /> class.
    /// </summary>
    public TwoFactorViewModel(
        IApiClient apiClient,
        ICountdownTimer countdownTimer,
        ILogger<TwoFactorViewModel> logger,
        bool isLocked = false)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _countdownTimer = countdownTimer ?? throw new ArgumentNullException(nameof(countdownTimer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IsLocked = isLocked;
        State = new ObservableState<TwoFactorState>(TwoFactorState.Idle);
        _countdownTimer.Tick += OnCountdownTick;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the current state of the 2FA flow.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<TwoFactorState> State { get; }

    /// <summary>
    ///     Gets or sets the OTP code typed by the user.
    ///     Set by the view via a <c>TextChanged</c> handler — no Two-Way binding needed.
    /// </summary>
    /// <value>
    ///     The OTP code typed by the user.
    ///     Set by the view via a <c>TextChanged</c> handler — no Two-Way binding needed.
    /// </value>
    public string OtpCode
    {
        get => _otpCode;
        set => SetField(ref _otpCode, value);
    }

    /// <summary>
    ///     Gets a value indicating whether a network operation is in progress.
    /// </summary>
    /// <value>
    ///     A value indicating whether a network operation is in progress.
    /// </value>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetField(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsInputEnabled));
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the OTP input and verify button are enabled.
    ///     Binds to both <c>OtpBox.IsEnabled</c> and <c>VerifyButton.IsEnabled</c>.
    /// </summary>
    /// <value>
    ///     A value indicating whether the OTP input and verify button are enabled.
    ///     Binds to both <c>OtpBox.IsEnabled</c> and <c>VerifyButton.IsEnabled</c>.
    /// </value>
    public bool IsInputEnabled => !_isLoading && !IsLocked;

    /// <summary>
    ///     Gets the current error message, or an empty string when there is no active error.
    /// </summary>
    /// <value>
    ///     The current error message, or an empty string when there is no active error.
    /// </value>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    /// <summary>
    ///     Gets a value indicating whether an error message is currently active.
    ///     Binds to <c>ErrorInfoBar.IsOpen</c>.
    /// </summary>
    /// <value>
    ///     A value indicating whether an error message is currently active.
    ///     Binds to <c>ErrorInfoBar.IsOpen</c>.
    /// </value>
    public bool HasError
    {
        get => _hasError;
        private set => SetField(ref _hasError, value);
    }

    /// <summary>
    ///     Gets the number of seconds remaining before the resend button becomes available.
    /// </summary>
    /// <value>
    ///     The number of seconds remaining before the resend button becomes available.
    /// </value>
    public int SecondsRemaining
    {
        get => _secondsRemaining;
        internal set
        {
            if (!SetField(ref _secondsRemaining, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CanResend));
            OnPropertyChanged(nameof(IsCountdownVisible));
            OnPropertyChanged(nameof(CountdownDisplayText));
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the user is allowed to request a new code.
    ///     Becomes <see langword="true" /> once the countdown reaches zero.
    /// </summary>
    /// <value>
    ///     A value indicating whether the user is allowed to request a new code.
    ///     Becomes <see langword="true" /> once the countdown reaches zero.
    /// </value>
    public bool CanResend => _secondsRemaining <= 0;

    /// <summary>
    ///     Gets a value indicating whether the countdown label should be displayed.
    /// </summary>
    /// <value>
    ///     A value indicating whether the countdown label should be displayed.
    /// </value>
    public bool IsCountdownVisible => _secondsRemaining > 0;

    /// <summary>
    ///     Gets the formatted countdown string shown next to the resend button,
    ///     e.g. <c>"Available in 27s"</c>.
    /// </summary>
    /// <value>
    ///     The formatted countdown string shown next to the resend button,
    ///     e.g. <c>"Available in 27s"</c>.
    /// </value>
    public string CountdownDisplayText => $"Available in {_secondsRemaining}s";

    /// <summary>
    ///     Gets a value indicating whether the user's input has been locked out
    ///     due to too many failed attempts.
    /// </summary>
    /// <value>
    ///     A value indicating whether the user's input has been locked out
    ///     due to too many failed attempts.
    /// </value>
    private bool IsLocked { get; }

    /// <summary>
    ///     Validates and submits the current <see cref="OtpCode" /> to the API.
    ///     Length validation (6 digits) is enforced here — not in the view.
    ///     Sets <see cref="TwoFactorState.Success" /> on a valid code, or
    ///     <see cref="TwoFactorState.InvalidOtp" /> when the code is rejected or the
    ///     session has expired.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task VerifyOtp()
    {
        ClearError();
        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length != OtpRequiredLength)
        {
            SetError(UserMessages.TwoFactor.InvalidCodeFormat);
            return;
        }

        IsLoading = true;
        State.SetValue(TwoFactorState.Verifying);
        int? userId = _apiClient.CurrentUserId;
        if (userId == null)
        {
            ApplyInvalidOtp();
            return;
        }

        var request = new VerifyOtpRequest
        {
            UserId = userId.Value,
            OtpCode = OtpCode,
        };
        ErrorOr<LoginSuccessResponse> result = await _apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(
            ApiEndpoints.VerifyOtp,
            request);
        result.Switch(
            response =>
            {
                _apiClient.SetToken(response.Token!);
                IsLoading = false;
                State.SetValue(TwoFactorState.Success);
            },
            errors =>
            {
                if (errors.First().Type != ErrorType.Unauthorized)
                {
                    _logger.LogError("VerifyOtp failed: {Errors}", errors);
                }

                ApplyInvalidOtp();
            });
    }

    /// <summary>
    ///     Requests a new OTP for the current user.
    ///     Does nothing if <see cref="CanResend" /> is <see langword="false" /> —
    ///     the view may call this unconditionally and the guard here prevents
    ///     duplicate or premature API calls.
    ///     Failures are logged but do not change the view state; resend is best-effort.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ResendOtp()
    {
        if (!CanResend)
        {
            return;
        }

        ClearError();
        _secondsRemaining = ResendCooldownSeconds;
        _countdownTimer.Start();
        State.SetValue(TwoFactorState.Idle);
        int? userId = _apiClient.CurrentUserId;
        if (userId == null)
        {
            return;
        }

        ErrorOr<object> result = await _apiClient.PostAsync<object?, object>(
            $"{ApiEndpoints.ResendOtp}?userId={userId.Value}",
            null);
        result.Switch(
            _ => { },
            errors => _logger.LogError("ResendOtp failed: {Errors}", errors));
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        if (_secondsRemaining > 0.0d)
        {
            _secondsRemaining--;
        }

        if (_secondsRemaining <= 0)
        {
            _countdownTimer.Stop();
        }
    }

    private void ApplyInvalidOtp()
    {
        IsLoading = false;
        SetError(UserMessages.TwoFactor.IncorrectCode);
        State.SetValue(TwoFactorState.InvalidOtp);
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Sets <paramref name="field" /> to <paramref name="value" /> and raises
    ///     <see cref="PropertyChanged" /> if the value actually changed.
    /// </summary>
    /// <returns><see langword="true" /> if the value changed; otherwise <see langword="false" />.</returns>
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
