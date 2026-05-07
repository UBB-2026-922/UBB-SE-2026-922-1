namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.Features.Authentication.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Coordinates OTP verification and resend operations for the two-factor flow.
/// </summary>
public partial class TwoFactorViewModel : INotifyPropertyChanged
{
    private const int ResendCooldownSeconds = 30;
    private const int OtpRequiredLength = 6;
    private readonly IAuthClientService _authClientService;
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
        IAuthClientService authClientService,
        ICountdownTimer countdownTimer,
        ILogger<TwoFactorViewModel> logger,
        bool isLocked = false)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _countdownTimer = countdownTimer ?? throw new ArgumentNullException(nameof(countdownTimer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IsLocked = isLocked;
        State = new ObservableState<TwoFactorState>(TwoFactorState.Idle);
        _countdownTimer.Tick += OnCountdownTick;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the current two-factor workflow state.
    /// </summary>
    public ObservableState<TwoFactorState> State { get; }

    /// <summary>
    ///     Gets or sets the OTP code entered by the user.
    /// </summary>
    public string OtpCode
    {
        get => _otpCode;
        set => SetField(ref _otpCode, value);
    }

    /// <summary>
    ///     Gets a value indicating whether a two-factor request is currently in progress.
    /// </summary>
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
    ///     Gets a value indicating whether OTP input controls should be enabled.
    /// </summary>
    public bool IsInputEnabled => !_isLoading && !IsLocked;

    /// <summary>
    ///     Gets the latest user-facing error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    /// <summary>
    ///     Gets a value indicating whether an error message is currently active.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set => SetField(ref _hasError, value);
    }

    /// <summary>
    ///     Gets or sets the remaining seconds until a resend is allowed.
    /// </summary>
    public int SecondsRemaining
    {
        get => _secondsRemaining;
        private set
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
    ///     Gets a value indicating whether a new OTP may be requested.
    /// </summary>
    public bool CanResend => _secondsRemaining <= 0;

    /// <summary>
    ///     Gets a value indicating whether the resend countdown should be shown.
    /// </summary>
    public bool IsCountdownVisible => _secondsRemaining > 0;

    /// <summary>
    ///     Gets the countdown text shown next to the resend action.
    /// </summary>
    public string CountdownDisplayText => $"Available in {_secondsRemaining}s";

    private bool IsLocked { get; }

    /// <summary>
    ///     Verifies the entered OTP with the backend.
    /// </summary>
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
        int? userId = _authClientService.CurrentUserId;
        if (userId == null)
        {
            ApplyInvalidOtp();
            return;
        }

        ErrorOr<LoginSuccessResponse> result = await _authClientService.VerifyOtpAsync(userId.Value, OtpCode);
        result.Switch(
            response =>
            {
                _authClientService.SetToken(response.Token!);
                IsLoading = false;
                State.SetValue(TwoFactorState.Success);
            },
            errors =>
            {
                if (errors.First().Type != ErrorType.Unauthorized)
                {
                    _logger.VerifyOtpFailed(errors);
                }

                ApplyInvalidOtp();
            });
    }

    /// <summary>
    ///     Requests a new OTP if the resend cooldown has expired.
    /// </summary>
    public async Task ResendOtp()
    {
        if (!CanResend)
        {
            return;
        }

        ClearError();
        SecondsRemaining = ResendCooldownSeconds;
        _countdownTimer.Start();
        State.SetValue(TwoFactorState.Idle);
        int? userId = _authClientService.CurrentUserId;
        if (userId == null)
        {
            return;
        }

        ErrorOr<object> result = await _authClientService.ResendOtpAsync(userId.Value);
        result.Switch(_ => { }, errors => _logger.ResendOtpFailed(errors));
    }

    private void OnCountdownTick(object? sender, EventArgs routedEventArgs)
    {
        if (SecondsRemaining > 0)
        {
            SecondsRemaining--;
        }

        if (SecondsRemaining <= 0)
        {
            _countdownTimer.StopTimer();
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
