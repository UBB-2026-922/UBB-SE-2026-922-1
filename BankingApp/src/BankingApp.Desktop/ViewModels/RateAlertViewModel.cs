namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Application.DTOs.RateAlerts;
using Services;
using Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Handles rate-alert listing, creation, and deletion for the desktop client.
/// </summary>
public partial class RateAlertViewModel : INotifyPropertyChanged
{
    private const decimal MinimumRate = 0m;
    private static readonly string[] _availableCurrencyCodes = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IRateAlertClientService _rateAlertClientService;
    private readonly ILogger<RateAlertViewModel> _logger;
    private ObservableCollection<RateAlertDto> _alerts = [];
    private string _baseCurrency = string.Empty;
    private string _targetCurrency = string.Empty;
    private string _targetRateText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBuyAlert;
    private bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertViewModel" /> class.
    /// </summary>
    public RateAlertViewModel(IRateAlertClientService rateAlertClientService, ILogger<RateAlertViewModel> logger)
    {
        _rateAlertClientService = rateAlertClientService ?? throw new ArgumentNullException(nameof(rateAlertClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        AvailableCurrencies = new ObservableCollection<string>(_availableCurrencyCodes);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the supported currency codes offered by the UI.
    /// </summary>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>
    ///     Gets or sets the currently loaded alerts.
    /// </summary>
    public ObservableCollection<RateAlertDto> Alerts
    {
        get => _alerts;
        set => SetProperty(ref _alerts, value);
    }

    /// <summary>
    ///     Gets or sets the selected base currency for a new alert.
    /// </summary>
    public string BaseCurrency
    {
        get => _baseCurrency;
        set => SetProperty(ref _baseCurrency, value);
    }

    /// <summary>
    ///     Gets or sets the selected target currency for a new alert.
    /// </summary>
    public string TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    /// <summary>
    ///     Gets or sets the raw target-rate text entered by the user.
    /// </summary>
    public string TargetRateText
    {
        get => _targetRateText;
        set => SetProperty(ref _targetRateText, value);
    }

    /// <summary>
    ///     Gets or sets the latest user-facing error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the alert is a buy alert.
    /// </summary>
    public bool IsBuyAlert
    {
        get => _isBuyAlert;
        set => SetProperty(ref _isBuyAlert, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether a rate-alert request is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    ///     Loads all alerts for the current user.
    /// </summary>
    public async Task LoadAlertsAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;
        try
        {
            int userId = _rateAlertClientService.CurrentUserId ?? 0;
            ErrorOr<System.Collections.Generic.List<RateAlertDto>> result =
                await _rateAlertClientService.GetAlertsAsync(userId);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.LoadFailed;
                _logger.LoadAlertsFailed(result.Errors);
                return;
            }

            Alerts = new ObservableCollection<RateAlertDto>(result.Value);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.LoadAlertsFailedUnexpected(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Creates a new rate alert from the current form state.
    /// </summary>
    public async Task CreateAlertAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(BaseCurrency) || string.IsNullOrWhiteSpace(TargetCurrency))
        {
            ErrorMessage = UserMessages.RateAlerts.CurrencyRequired;
            return;
        }

        if (string.Equals(BaseCurrency, TargetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = UserMessages.RateAlerts.CurrenciesMustDiffer;
            return;
        }

        string cleanedRateText = TargetRateText.Trim();
        if (cleanedRateText.Contains(',') && cleanedRateText.Contains('.'))
        {
            ErrorMessage = UserMessages.RateAlerts.InvalidNumberFormat;
            return;
        }

        cleanedRateText = cleanedRateText.Replace('.', ',');
        if (!decimal.TryParse(
                cleanedRateText,
                System.Globalization.NumberStyles.Number,
                new System.Globalization.CultureInfo("ro-RO"),
                out decimal parsedRate) || parsedRate <= MinimumRate)
        {
            ErrorMessage = UserMessages.RateAlerts.InvalidTargetRate;
            return;
        }

        IsLoading = true;
        try
        {
            var newAlert = new RateAlertDto
            {
                UserId = _rateAlertClientService.CurrentUserId ?? 0,
                BaseCurrency = BaseCurrency,
                TargetCurrency = TargetCurrency,
                TargetRate = parsedRate,
                IsBuyAlert = IsBuyAlert,
            };

            ErrorOr<RateAlertDto> result = await _rateAlertClientService.CreateAlertAsync(newAlert);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.CreateFailed;
                _logger.CreateAlertFailed(result.Errors);
                return;
            }

            Alerts.Add(result.Value);
            BaseCurrency = string.Empty;
            TargetCurrency = string.Empty;
            TargetRateText = string.Empty;
            ErrorMessage = string.Empty;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.CreateAlertFailedUnexpected(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Deletes the specified alert.
    /// </summary>
    public async Task DeleteAlertAsync(int alertId)
    {
        ErrorMessage = string.Empty;
        try
        {
            ErrorOr<Success> result = await _rateAlertClientService.DeleteAlertAsync(alertId);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.DeleteFailed;
                _logger.DeleteAlertFailed(result.Errors);
                return;
            }

            RateAlertDto? toRemove = Alerts.FirstOrDefault(alert => alert.Id == alertId);
            if (toRemove != null)
            {
                Alerts.Remove(toRemove);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.DeleteAlertFailedUnexpected(exception);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
