namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Handles rate-alert listing, creation, and deletion for the desktop client.</summary>
public partial class RateAlertViewModel : ObservableObject
{
    private const decimal MinimumRate = 0m;
    private static readonly string[] _availableCurrencyCodes = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IRateAlertClientService _rateAlertClientService;
    private readonly ILogger<RateAlertViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="RateAlertViewModel"/> class.</summary>
    public RateAlertViewModel(IRateAlertClientService rateAlertClientService, ILogger<RateAlertViewModel> logger)
    {
        _rateAlertClientService = rateAlertClientService ?? throw new ArgumentNullException(nameof(rateAlertClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        AvailableCurrencies = new ObservableCollection<string>(_availableCurrencyCodes);
        Alerts = [];
    }

    /// <summary>Gets the supported currency codes offered by the UI.</summary>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>Gets or sets the currently loaded alerts.</summary>
    [ObservableProperty]
    public partial ObservableCollection<ForexRateAlertDto> Alerts { get; set; } = default!;

    /// <summary>Gets or sets the selected base currency for a new alert.</summary>
    [ObservableProperty]
    public partial string BaseCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the selected target currency for a new alert.</summary>
    [ObservableProperty]
    public partial string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw target-rate text entered by the user.</summary>
    [ObservableProperty]
    public partial string TargetRateText { get; set; } = string.Empty;

    /// <summary>Gets or sets the latest user-facing error message.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the alert is a buy alert.</summary>
    [ObservableProperty]
    public partial bool IsBuyAlert { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether a rate-alert request is in progress.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; } = default!;

    /// <summary>Loads all alerts for the current user.</summary>
    public async Task LoadAlertsAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;
        try
        {
            int userId = _rateAlertClientService.CurrentUserId ?? 0;
            ErrorOr<System.Collections.Generic.List<ForexRateAlertDto>> result =
                await _rateAlertClientService.GetAlertsAsync(userId);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.LoadFailed;
                _logger.LoadAlertsFailed(result.Errors);
                return;
            }

            Alerts = new ObservableCollection<ForexRateAlertDto>(result.Value);
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

    /// <summary>Creates a new rate alert from the current form state.</summary>
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
            var newAlert = new ForexRateAlertDto
            {
                UserId = _rateAlertClientService.CurrentUserId ?? 0,
                BaseCurrency = BaseCurrency,
                TargetCurrency = TargetCurrency,
                TargetRate = parsedRate,
                IsBuyAlert = IsBuyAlert,
            };

            ErrorOr<ForexRateAlertDto> result = await _rateAlertClientService.CreateAlertAsync(newAlert);

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

    /// <summary>Deletes the specified alert.</summary>
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

            ForexRateAlertDto? toRemove = Alerts.FirstOrDefault(alert => alert.Id == alertId);
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
}
