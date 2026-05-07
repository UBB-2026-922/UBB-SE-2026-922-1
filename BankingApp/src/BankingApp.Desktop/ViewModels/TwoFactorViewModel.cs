using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Auth;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableState<TwoFactorState> State { get; }

    public string OtpCode
    {
        get => _otpCode;
        set => SetField(ref _otpCode, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetField(ref _isLoading, value)) OnPropertyChanged(nameof(IsInputEnabled));
        }
    }

    public bool IsInputEnabled => !_isLoading && !IsLocked;

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetField(ref _hasError, value);
    }

    public int SecondsRemaining
    {
        get => _secondsRemaining;
        internal set
        {
            if (!SetField(ref _secondsRemaining, value)) return;

            OnPropertyChanged(nameof(CanResend));
            OnPropertyChanged(nameof(IsCountdownVisible));
            OnPropertyChanged(nameof(CountdownDisplayText));
        }
    }

    public bool CanResend => _secondsRemaining <= 0;

    public bool IsCountdownVisible => _secondsRemaining > 0;

    public string CountdownDisplayText => $"Available in {_secondsRemaining}s";

    private bool IsLocked { get; }

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
                    _logger.VerifyOtpFailed(errors);

                ApplyInvalidOtp();
            });
    }

    public async Task ResendOtp()
    {
        if (!CanResend) return;

        ClearError();
        SecondsRemaining = ResendCooldownSeconds;
        _countdownTimer.Start();
        State.SetValue(TwoFactorState.Idle);
        int? userId = _authClientService.CurrentUserId;
        if (userId == null) return;

        ErrorOr<object> result = await _authClientService.ResendOtpAsync(userId.Value);
        result.Switch(
            _ => { },
            errors => _logger.ResendOtpFailed(errors));
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        if (SecondsRemaining > 0) SecondsRemaining--;

        if (SecondsRemaining <= 0) _countdownTimer.StopTimer();
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
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
