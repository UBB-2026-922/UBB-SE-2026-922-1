namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using BankingApp.Application.Features.Authentication.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Coordinates OTP verification and resend operations for the two-factor flow.</summary>
public partial class TwoFactorViewModel : ObservableObject
{
    private const int ResendCooldownSeconds = 30;
    private const int OtpRequiredLength = 6;
    private readonly IAuthClientService _authClientService;
    private readonly ICountdownTimer _countdownTimer;
    private readonly ILogger<TwoFactorViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="TwoFactorViewModel"/> class.</summary>
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
        _countdownTimer.Tick += OnCountdownTick;
    }

    /// <summary>Gets or sets the current two-factor workflow state.</summary>
    [ObservableProperty]
    public partial TwoFactorState State { get; set; } = TwoFactorState.Idle;

    /// <summary>Gets or sets the OTP code entered by the user.</summary>
    [ObservableProperty]
    public partial string OtpCode { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether a two-factor request is currently in progress.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInputEnabled))]
public partial bool IsLoading { get; set; } = default!;

    /// <summary>Gets a value indicating whether OTP input controls should be enabled.</summary>
    public bool IsInputEnabled => !IsLoading && !IsLocked;

    /// <summary>Gets or sets the latest user-facing error message.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether an error message is currently active.</summary>
    [ObservableProperty]
    public partial bool HasError { get; set; } = default!;

    /// <summary>Gets or sets the remaining seconds until a resend is allowed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanResend))]
    [NotifyPropertyChangedFor(nameof(IsCountdownVisible))]
    [NotifyPropertyChangedFor(nameof(CountdownDisplayText))]
public partial int SecondsRemaining { get; set; } = default!;

    /// <summary>Gets a value indicating whether a new OTP may be requested.</summary>
    public bool CanResend => SecondsRemaining <= 0;

    /// <summary>Gets a value indicating whether the resend countdown should be shown.</summary>
    public bool IsCountdownVisible => SecondsRemaining > 0;

    /// <summary>Gets the countdown text shown next to the resend action.</summary>
    public string CountdownDisplayText => $"Available in {SecondsRemaining}s";

    private bool IsLocked { get; }

    /// <summary>Verifies the entered OTP with the backend.</summary>
    public async Task VerifyOtp()
    {
        ClearError();
        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length != OtpRequiredLength)
        {
            SetError(UserMessages.TwoFactor.InvalidCodeFormat);
            return;
        }

        IsLoading = true;
        State = TwoFactorState.Verifying;
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
                State = TwoFactorState.Success;
            },
            errors =>
            {
                if (errors[0].Type != ErrorType.Unauthorized)
                {
                    _logger.VerifyOtpFailed(errors);
                }

                ApplyInvalidOtp();
            });
    }

    /// <summary>Requests a new OTP if the resend cooldown has expired.</summary>
    public async Task ResendOtp()
    {
        if (!CanResend)
        {
            return;
        }

        ClearError();
        SecondsRemaining = ResendCooldownSeconds;
        _countdownTimer.Start();
        State = TwoFactorState.Idle;
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
        State = TwoFactorState.InvalidOtp;
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
}
