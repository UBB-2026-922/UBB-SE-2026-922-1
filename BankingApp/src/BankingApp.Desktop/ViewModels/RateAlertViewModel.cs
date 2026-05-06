// <copyright file="RateAlertViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     View model for the Rate Alerts page. Allows the user to create, view,
///     and delete exchange-rate alerts via the API.
/// </summary>
public partial class RateAlertViewModel : INotifyPropertyChanged
{
    private const decimal MinimumRate = 0m;
    private static readonly string[] _availableCurrencyCodes = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IApiClient _apiClient;
    private readonly ILogger<RateAlertViewModel> _logger;

    private ObservableCollection<RateAlertDto> _alerts = new();
    private string _baseCurrency = string.Empty;
    private string _targetCurrency = string.Empty;
    private string _targetRateText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBuyAlert;
    private bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client for backend communication.</param>
    /// <param name="logger">Logger for rate alert errors.</param>
    public RateAlertViewModel(IApiClient apiClient, ILogger<RateAlertViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        AvailableCurrencies = new ObservableCollection<string>(_availableCurrencyCodes);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the available currencies for selection.</summary>
    /// <value>Gets or sets the current value.</value>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>Gets or sets the collection of the user's rate alerts.</summary>
    /// <value>Gets or sets the current value.</value>
    public ObservableCollection<RateAlertDto> Alerts
    {
        get => _alerts;
        set => SetProperty(ref _alerts, value);
    }

    /// <summary>Gets or sets the ISO 4217 base currency code for a new alert.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BaseCurrency
    {
        get => _baseCurrency;
        set => SetProperty(ref _baseCurrency, value);
    }

    /// <summary>Gets or sets the ISO 4217 target currency code for a new alert.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    /// <summary>Gets or sets the raw text from the target rate input field.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetRateText
    {
        get => _targetRateText;
        set => SetProperty(ref _targetRateText, value);
    }

    /// <summary>Gets or sets the most recent error message to display.</summary>
    /// <value>Gets or sets the current value.</value>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Gets or sets a value indicating whether this is a buy alert.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsBuyAlert
    {
        get => _isBuyAlert;
        set => SetProperty(ref _isBuyAlert, value);
    }

    /// <summary>Gets or sets a value indicating whether an API call is in progress.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    ///     Loads the current user's rate alerts from the API.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadAlertsAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;
        try
        {
            int userId = _apiClient.CurrentUserId ?? 0;
            string endpoint = $"{ApiEndpoints.RateAlerts}?userId={userId}";
            ErrorOr<List<RateAlertDto>> result = await _apiClient.GetAsync<List<RateAlertDto>>(endpoint);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.LoadFailed;
                _logger.LogError("Load alerts failed: {Errors}", result.Errors);
                return;
            }

            Alerts = new ObservableCollection<RateAlertDto>(result.Value);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.LogError(exception, "Load alerts failed unexpectedly");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Validates inputs and creates a new rate alert via the API.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
                UserId = _apiClient.CurrentUserId ?? 0,
                BaseCurrency = BaseCurrency,
                TargetCurrency = TargetCurrency,
                TargetRate = parsedRate,
                IsBuyAlert = IsBuyAlert
            };

            ErrorOr<RateAlertDto> result =
                await _apiClient.PostAsync<RateAlertDto, RateAlertDto>(ApiEndpoints.RateAlerts, newAlert);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.CreateFailed;
                _logger.LogError("Create alert failed: {Errors}", result.Errors);
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
            _logger.LogError(exception, "Create alert failed unexpectedly");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Deletes the specified rate alert via the API.
    /// </summary>
    /// <param name="alertId">The identifier of the alert to delete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteAlertAsync(int alertId)
    {
        ErrorMessage = string.Empty;
        try
        {
            string endpoint = $"{ApiEndpoints.RateAlerts}/{alertId}";
            ErrorOr<Success> result = await _apiClient.DeleteAsync(endpoint);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.RateAlerts.DeleteFailed;
                _logger.LogError("Delete alert failed: {Errors}", result.Errors);
                return;
            }

            RateAlertDto? toRemove = Alerts.FirstOrDefault(alert => alert.Id == alertId);
            if (toRemove != null) Alerts.Remove(toRemove);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.LogError(exception, "Delete alert failed unexpectedly");
        }
    }

    /// <summary>Raises PropertyChanged for the given property name.</summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>Sets the backing field and raises PropertyChanged when the value differs.</summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name (auto-filled by the compiler).</param>
    /// <returns><see langword="true"/> if the value changed; otherwise, <see langword="false"/>.</returns>
    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
