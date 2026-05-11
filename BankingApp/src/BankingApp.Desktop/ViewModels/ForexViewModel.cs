namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Manages exchange-rate preview and foreign-exchange execution for the desktop client.</summary>
public partial class ForexViewModel : ObservableObject
{
    private const int InitialStep = 1;
    private const int PreviewStep = 2;
    private const int ResultStep = 4;
    private const decimal MinimumAmount = 0m;

    private static readonly string[] _collection = ["EUR", "USD", "GBP", "RON", "CHF", "JPY"];

    private readonly IForexClientService _forexClientService;
    private readonly ILogger<ForexViewModel> _logger;

    // Not observable — derived from AmountText via OnAmountTextChanged
    private decimal _amount;

    /// <summary>Initializes a new instance of the <see cref="ForexViewModel"/> class.</summary>
    public ForexViewModel(IForexClientService forexClientService, ILogger<ForexViewModel> logger)
    {
        _forexClientService = forexClientService ?? throw new ArgumentNullException(nameof(forexClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CurrentStep = InitialStep;
        AvailableCurrencies = new ObservableCollection<string>(_collection);
    }

    /// <summary>Gets the currencies available for exchange.</summary>
    public ObservableCollection<string> AvailableCurrencies { get; }

    /// <summary>Gets or sets the current step in the exchange flow.</summary>
    [ObservableProperty]
    public partial int CurrentStep { get; set; } = default!;

    /// <summary>Gets or sets the currency being sold.</summary>
    [ObservableProperty]
    public partial string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the currency being bought.</summary>
    [ObservableProperty]
    public partial string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the source amount as entered by the user.</summary>
    [ObservableProperty]
    public partial string AmountText { get; set; } = string.Empty;

    /// <summary>Gets the parsed source amount.</summary>
    public decimal Amount => _amount;

    /// <summary>Gets or sets the live exchange rate shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal LiveRate { get; set; } = default!;

    /// <summary>Gets or sets the commission shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal Commission { get; set; } = default!;

    /// <summary>Gets or sets the target amount shown in the preview.</summary>
    [ObservableProperty]
    public partial decimal TargetAmount { get; set; } = default!;

    /// <summary>Gets or sets the reference of the completed exchange transaction.</summary>
    [ObservableProperty]
    public partial string TransactionReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the current user-facing error message.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the exchange flow is busy.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; } = default!;

    partial void OnAmountTextChanged(string value)
    {
        _amount = decimal.TryParse(value, out decimal parsed) ? parsed : MinimumAmount;
    }

    /// <summary>Loads an exchange preview for the current currencies and amount.</summary>
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

    /// <summary>Executes the exchange using the values currently shown in the flow.</summary>
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

            ErrorOr<ForexTransactionResponse> result = await _forexClientService.ExecuteExchangeAsync(request);

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

    /// <summary>Resets the exchange flow back to its initial state.</summary>
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
}
