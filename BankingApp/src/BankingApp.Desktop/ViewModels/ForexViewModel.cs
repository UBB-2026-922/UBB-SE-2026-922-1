using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Exchange;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     View model for the FX currency exchange page. Drives a multi-step wizard
///     that lets the user preview rates, lock a rate, and execute the exchange.
/// </summary>
public partial class ForexViewModel : INotifyPropertyChanged
{
    private const int InitialStep = 1;
    private const int PreviewStep = 2;
    private const int ConfirmStep = 3;
    private const int ResultStep = 4;
    private const decimal MinimumAmount = 0m;

    private static readonly string[] _collection = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IApiClient _apiClient;
    private readonly ILogger<ForexViewModel> _logger;

    private int _currentStep;
    private string _sourceCurrency = string.Empty;
    private string _targetCurrency = string.Empty;
    private string _amountText = string.Empty;
    private decimal _amount;
    private decimal _liveRate;
    private decimal _commission;
    private decimal _targetAmount;
    private string _transactionReference = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client for backend communication.</param>
    /// <param name="logger">Logger for exchange errors.</param>
    public ForexViewModel(IApiClient apiClient, ILogger<ForexViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentStep = InitialStep;
        AvailableCurrencies = new ObservableCollection<string>(_collection);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the available currencies for selection.</summary>
    /// <value>Gets or sets the current value.</value>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>Gets or sets the current wizard step.</summary>
    /// <value>Gets or sets the current value.</value>
    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    /// <summary>Gets or sets the ISO 4217 source currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string SourceCurrency
    {
        get => _sourceCurrency;
        set => SetProperty(ref _sourceCurrency, value);
    }

    /// <summary>Gets or sets the ISO 4217 target currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    /// <summary>Gets or sets the raw text from the amount input field.</summary>
    /// <value>Gets or sets the current value.</value>
    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
                _amount = decimal.TryParse(value, out decimal parsed) ? parsed : MinimumAmount;
        }
    }

    /// <summary>Gets the parsed decimal amount.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount => _amount;

    /// <summary>Gets or sets the live exchange rate.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal LiveRate
    {
        get => _liveRate;
        set => SetProperty(ref _liveRate, value);
    }

    /// <summary>Gets or sets the commission for this exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Commission
    {
        get => _commission;
        set => SetProperty(ref _commission, value);
    }

    /// <summary>Gets or sets the computed target amount after conversion.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetAmount
    {
        get => _targetAmount;
        set => SetProperty(ref _targetAmount, value);
    }

    /// <summary>Gets or sets the reference number of the completed transaction.</summary>
    /// <value>Gets or sets the current value.</value>
    public string TransactionReference
    {
        get => _transactionReference;
        set => SetProperty(ref _transactionReference, value);
    }

    /// <summary>Gets or sets the most recent error message to display.</summary>
    /// <value>Gets or sets the current value.</value>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Gets or sets a value indicating whether an API call is in progress.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    ///     Fetches a rate preview from the API and advances the wizard to the preview step.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadPreviewAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SourceCurrency) || string.IsNullOrWhiteSpace(TargetCurrency))
        {
            ErrorMessage = UserMessages.Exchange.CurrencyRequired;
            return;
        }

        if (_amount <= MinimumAmount)
        {
            ErrorMessage = UserMessages.Exchange.AmountRequired;
            return;
        }

        IsLoading = true;
        try
        {
            string endpoint =
                $"{ApiEndpoints.ExchangePreview}?sourceCurrency={SourceCurrency}&targetCurrency={TargetCurrency}&amount={_amount}";
            ErrorOr<ExchangeTransactionResponse> result =
                await _apiClient.GetAsync<ExchangeTransactionResponse>(endpoint);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.PreviewFailed;
                _logger.RatePreviewFailed(result.Errors);
                return;
            }

            ExchangeTransactionResponse preview = result.Value;
            LiveRate = preview.ExchangeRate;
            Commission = preview.Commission;
            TargetAmount = preview.TargetAmount;
            CurrentStep = PreviewStep;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.RatePreviewFailedUnexpected(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Executes the currency exchange via the API and advances to the result step.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteExchangeAsync()
    {
        ErrorMessage = string.Empty;

        if (_amount <= MinimumAmount)
        {
            ErrorMessage = UserMessages.Exchange.AmountRequired;
            return;
        }

        IsLoading = true;
        try
        {
            var request = new ExchangeTransactionRequest
            {
                UserId = _apiClient.CurrentUserId ?? 0,
                SourceCurrency = SourceCurrency,
                TargetCurrency = TargetCurrency,
                SourceAmount = _amount
            };

            ErrorOr<ExchangeTransactionResponse> result =
                await _apiClient.PostAsync<ExchangeTransactionRequest, ExchangeTransactionResponse>(
                    ApiEndpoints.ExchangeExecute, request);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.ExecuteFailed;
                _logger.ExchangeExecutionFailed(result.Errors);
                return;
            }

            TransactionReference = $"TX-{result.Value.Id}";
            CurrentStep = ResultStep;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            _logger.ExchangeExecutionFailedUnexpected(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Resets all state and returns the wizard to the first step.
    /// </summary>
    public void Reset()
    {
        SourceCurrency = string.Empty;
        TargetCurrency = string.Empty;
        AmountText = string.Empty;
        LiveRate = 0;
        Commission = 0;
        TargetAmount = 0;
        TransactionReference = string.Empty;
        ErrorMessage = string.Empty;
        CurrentStep = InitialStep;
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
