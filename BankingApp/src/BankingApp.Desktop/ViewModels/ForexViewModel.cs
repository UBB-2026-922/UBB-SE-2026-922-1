namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages exchange-rate preview and foreign-exchange execution for the desktop client.
/// </summary>
public partial class ForexViewModel : INotifyPropertyChanged
{
    private const int InitialStep = 1;
    private const int PreviewStep = 2;
    private const int ConfirmStep = 3;
    private const int ResultStep = 4;
    private const decimal MinimumAmount = 0m;

    private static readonly string[] _collection = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IForexClientService _forexClientService;
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
    /// Initializes a new instance of the <see cref="ForexViewModel"/> class.
    /// </summary>
    /// <param name="forexClientService">Provides exchange preview and execution operations.</param>
    /// <param name="logger">Writes operational diagnostics for the exchange flow.</param>
    public ForexViewModel(IForexClientService forexClientService, ILogger<ForexViewModel> logger)
    {
        _forexClientService = forexClientService ?? throw new ArgumentNullException(nameof(forexClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentStep = InitialStep;
        AvailableCurrencies = new ObservableCollection<string>(_collection);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the currencies available for exchange.
    /// </summary>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>
    /// Gets or sets the current step in the exchange flow.
    /// </summary>
    public int CurrentStep
    {
        get => _currentStep;
        private set => SetProperty(ref _currentStep, value);
    }

    /// <summary>
    /// Gets or sets the currency being sold.
    /// </summary>
    public string SourceCurrency
    {
        get => _sourceCurrency;
        set => SetProperty(ref _sourceCurrency, value);
    }

    /// <summary>
    /// Gets or sets the currency being bought.
    /// </summary>
    public string TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    /// <summary>
    /// Gets or sets the source amount as entered by the user.
    /// </summary>
    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                _amount = decimal.TryParse(value, out decimal parsed) ? parsed : MinimumAmount;
            }
        }
    }

    /// <summary>
    /// Gets the parsed source amount.
    /// </summary>
    public decimal Amount => _amount;

    /// <summary>
    /// Gets or sets the live exchange rate shown in the preview.
    /// </summary>
    public decimal LiveRate
    {
        get => _liveRate;
        private set => SetProperty(ref _liveRate, value);
    }

    /// <summary>
    /// Gets or sets the commission shown in the preview.
    /// </summary>
    public decimal Commission
    {
        get => _commission;
        private set => SetProperty(ref _commission, value);
    }

    /// <summary>
    /// Gets or sets the target amount shown in the preview.
    /// </summary>
    public decimal TargetAmount
    {
        get => _targetAmount;
        private set => SetProperty(ref _targetAmount, value);
    }

    /// <summary>
    /// Gets or sets the reference of the completed exchange transaction.
    /// </summary>
    public string TransactionReference
    {
        get => _transactionReference;
        private set => SetProperty(ref _transactionReference, value);
    }

    /// <summary>
    /// Gets or sets the current user-facing error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the exchange flow is busy.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Loads an exchange preview for the current currencies and amount.
    /// </summary>
    /// <returns>A task that completes when the preview request finishes.</returns>
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
            ErrorOr<ForexTransactionResponse> result =
                await _forexClientService.GetPreviewAsync(SourceCurrency, TargetCurrency, _amount);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Exchange.PreviewFailed;
                _logger.RatePreviewFailed(result.Errors);
                return;
            }

            ForexTransactionResponse preview = result.Value;
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
    /// Executes the exchange using the values currently shown in the flow.
    /// </summary>
    /// <returns>A task that completes when the exchange request finishes.</returns>
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
            var request = new ForexTransactionRequest
            {
                UserId = _forexClientService.CurrentUserId ?? 0,
                SourceCurrency = SourceCurrency,
                TargetCurrency = TargetCurrency,
                SourceAmount = _amount,
            };

            ErrorOr<ForexTransactionResponse> result =
                await _forexClientService.ExecuteExchangeAsync(request);

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
    /// Resets the exchange flow back to its initial state.
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

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
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
