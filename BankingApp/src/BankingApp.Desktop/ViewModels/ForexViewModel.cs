using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Exchange;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

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

    public ForexViewModel(IForexClientService forexClientService, ILogger<ForexViewModel> logger)
    {
        _forexClientService = forexClientService ?? throw new ArgumentNullException(nameof(forexClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentStep = InitialStep;
        AvailableCurrencies = new ObservableCollection<string>(_collection);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> AvailableCurrencies { get; }

    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    public string SourceCurrency
    {
        get => _sourceCurrency;
        set => SetProperty(ref _sourceCurrency, value);
    }

    public string TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
                _amount = decimal.TryParse(value, out decimal parsed) ? parsed : MinimumAmount;
        }
    }

    public decimal Amount => _amount;

    public decimal LiveRate
    {
        get => _liveRate;
        set => SetProperty(ref _liveRate, value);
    }

    public decimal Commission
    {
        get => _commission;
        set => SetProperty(ref _commission, value);
    }

    public decimal TargetAmount
    {
        get => _targetAmount;
        set => SetProperty(ref _targetAmount, value);
    }

    public string TransactionReference
    {
        get => _transactionReference;
        set => SetProperty(ref _transactionReference, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

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
            ErrorOr<ExchangeTransactionResponse> result =
                await _forexClientService.GetPreviewAsync(SourceCurrency, TargetCurrency, _amount);

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
                UserId = _forexClientService.CurrentUserId ?? 0,
                SourceCurrency = SourceCurrency,
                TargetCurrency = TargetCurrency,
                SourceAmount = _amount,
            };

            ErrorOr<ExchangeTransactionResponse> result =
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

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
