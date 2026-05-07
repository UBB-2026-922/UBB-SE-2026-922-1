using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.DTOs.Billers;
using BankingApp.Desktop.Commands;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.Views;
using ErrorOr;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.ViewModels;

public partial class BillPayViewModel : INotifyPropertyChanged
{
    private const int SelectBillerStep = 1;
    private const int PaymentDetailsStep = 2;
    private const int TwoFactorAuthenticationStep = 3;
    private const int ReviewAndConfirmStep = 4;
    private const int PaymentResultStep = 5;
    private const int MinimumTwoFactorToken = 100000;
    private const int MaximumTwoFactorTokenExclusive = 1000000;
    private const int MinimumBillers = 0;
    private const int MinimumAmount = 0;
    private const int NoFee = 0;

    private readonly IBillPaymentClientService _billPaymentClientService;
    private readonly IAppNavigationService _navigationService;

    private int _currentStep;
    private ObservableCollection<BillerDto> _billers;
    private ObservableCollection<SavedBillerDto> _savedBillers;
    private ObservableCollection<AccountDto> _accounts;
    private BillerDto? _selectedBiller;
    private string _searchQuery = string.Empty;
    private string? _selectedCategory;
    private string _billerReference = string.Empty;
    private decimal _amount;
    private bool _isPayInFull;
    private AccountDto? _selectedAccount;
    private decimal _fee;
    private string _receiptNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _requires2Fa;
    private bool _is2FaConfirmed;
    private string _twoFaToken = string.Empty;
    private bool _shouldSaveBiller;

    public BillPayViewModel(IBillPaymentClientService billPaymentClientService, IAppNavigationService navigationService)
    {
        _billPaymentClientService = billPaymentClientService;
        _navigationService = navigationService;

        _billers = new ObservableCollection<BillerDto>();
        _savedBillers = new ObservableCollection<SavedBillerDto>();
        _accounts = new ObservableCollection<AccountDto>();
        _currentStep = SelectBillerStep;

        SearchCommand = new RelayCommand(unusedParameter => ExecuteSearch());
        SelectBillerCommand = new RelayCommand(ExecuteSelectBiller);
        NextStepCommand = new RelayCommand(unusedParameter => ExecuteNextStep());
        BackCommand = new RelayCommand(unusedParameter => ExecuteBack());
        PayAnotherBillCommand = new RelayCommand(unusedParameter => ResetForm());
        PayBillCommand = new AsyncRelayCommand(unusedParameter => ExecutePayBillAsync());
        CancelCommand = new RelayCommand(unusedParameter =>
            _navigationService.NavigateToContent<DashboardView>());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        set => SetProperty(ref _billers, value);
    }

    public ObservableCollection<SavedBillerDto> SavedBillers
    {
        get => _savedBillers;
        set
        {
            if (SetProperty(ref _savedBillers, value))
            {
                OnPropertyChanged(nameof(HasSavedBillers));
                OnPropertyChanged(nameof(SavedBillersVisibility));
            }
        }
    }

    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    public BillerDto? SelectedBiller
    {
        get => _selectedBiller;
        set
        {
            if (SetProperty(ref _selectedBiller, value))
            {
                ApplySavedDefaultsForSelectedBiller();
                OnPropertyChanged(nameof(SelectedBillerName));
            }
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public string? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value)) ExecuteSearch();
        }
    }

    public string BillerReference
    {
        get => _billerReference;
        set => SetProperty(ref _billerReference, value);
    }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                OnPropertyChanged(nameof(ReviewAmountText));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TotalText));
            }
        }
    }

    public double AmountAsDouble
    {
        get => (double)_amount;
        set => Amount = (decimal)value;
    }

    public bool IsPayInFull
    {
        get => _isPayInFull;
        set => SetProperty(ref _isPayInFull, value);
    }

    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    public decimal Fee
    {
        get => _fee;
        set
        {
            if (SetProperty(ref _fee, value))
            {
                OnPropertyChanged(nameof(ReviewFeeText));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TotalText));
            }
        }
    }

    public string ReceiptNumber
    {
        get => _receiptNumber;
        set => SetProperty(ref _receiptNumber, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(ErrorMessageVisibility));
        }
    }

    public bool Requires2Fa
    {
        get => _requires2Fa;
        set => SetProperty(ref _requires2Fa, value);
    }

    public bool Is2FaConfirmed
    {
        get => _is2FaConfirmed;
        set => SetProperty(ref _is2FaConfirmed, value);
    }

    public string TwoFaToken
    {
        get => _twoFaToken;
        set => SetProperty(ref _twoFaToken, value);
    }

    public bool ShouldSaveBiller
    {
        get => _shouldSaveBiller;
        set => SetProperty(ref _shouldSaveBiller, value);
    }

    public bool HasSavedBillers => SavedBillers != null && SavedBillers.Count > MinimumBillers;

    public Visibility SavedBillersVisibility =>
        HasSavedBillers ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ErrorMessageVisibility =>
        string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    public string SelectedBillerName =>
        SelectedBiller?.Name ?? "No biller selected";

    public string ReviewAmountText =>
        Amount > MinimumAmount ? $"{Amount:0.00} RON" : "No amount entered";

    public string ReviewFeeText => $"{Fee:0.00} RON";

    public decimal Total => Amount + Fee;

    public string TotalText => $"{Total:0.00} RON";

    public ICommand SearchCommand { get; }

    public ICommand SelectBillerCommand { get; }

    public ICommand NextStepCommand { get; }

    public ICommand BackCommand { get; }

    public ICommand PayAnotherBillCommand { get; }

    public ICommand PayBillCommand { get; }

    public ICommand CancelCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ResetFormStateOnly();

            ErrorOr<List<BillerDto>> billersResult = await _billPaymentClientService.GetBillersAsync();
            if (!billersResult.IsError) Billers = new ObservableCollection<BillerDto>(billersResult.Value);

            ErrorOr<List<SavedBillerDto>> savedResult = await _billPaymentClientService.GetSavedBillersAsync();
            if (!savedResult.IsError) SavedBillers = new ObservableCollection<SavedBillerDto>(savedResult.Value);

            ErrorOr<List<AccountDto>> accountsResult = await _billPaymentClientService.GetAccountsAsync();
            if (!accountsResult.IsError) Accounts = new ObservableCollection<AccountDto>(accountsResult.Value);
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    internal void ExecuteSearch()
    {
        try
        {
            ErrorMessage = string.Empty;
            string query = SearchQuery ?? string.Empty;

            Task<ErrorOr<List<BillerDto>>> task = _billPaymentClientService.GetBillersAsync(query, SelectedCategory);
            task.ContinueWith(
                completedTask =>
                {
                    if (!completedTask.Result.IsError)
                        Billers = new ObservableCollection<BillerDto>(completedTask.Result.Value);
                },
                TaskScheduler.Default);
        }
        catch (Exception searchException)
        {
            ErrorMessage = $"Search failed: {searchException.Message}";
        }
    }

    internal void ExecuteSelectBiller(object? parameter)
    {
        ErrorMessage = string.Empty;

        if (parameter is BillerDto biller)
        {
            SelectedBiller = biller;
            CurrentStep = PaymentDetailsStep;
            return;
        }

        if (parameter is SavedBillerDto savedBiller)
        {
            SelectedBiller = savedBiller.ToBiller();
            if (!string.IsNullOrWhiteSpace(savedBiller.DefaultReference))
                BillerReference = savedBiller.DefaultReference!;

            CurrentStep = PaymentDetailsStep;
        }
    }

    internal void ExecuteNextStep()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep == SelectBillerStep)
        {
            if (SelectedBiller == null)
            {
                ErrorMessage = "Please select a biller.";
                return;
            }

            CurrentStep = PaymentDetailsStep;
            return;
        }

        if (CurrentStep == PaymentDetailsStep)
        {
            if (SelectedBiller == null)
            {
                ErrorMessage = "Please select a biller.";
                return;
            }

            if (string.IsNullOrWhiteSpace(BillerReference))
            {
                ErrorMessage = "Please enter a biller reference.";
                return;
            }

            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select a source account.";
                return;
            }

            if (Amount <= MinimumAmount)
            {
                ErrorMessage = "Please enter a valid amount.";
                return;
            }

            ErrorOr<FeeResponse> feeResult = _billPaymentClientService
                .GetFeeAsync(Amount)
                .GetAwaiter().GetResult();
            Fee = !feeResult.IsError ? feeResult.Value.Fee : 0m;

            ErrorOr<RequiresTwoFaResponse> twoFaResult = _billPaymentClientService
                .GetRequires2FaAsync(Amount)
                .GetAwaiter().GetResult();
            Requires2Fa = !twoFaResult.IsError && twoFaResult.Value.Required;

            CurrentStep = Requires2Fa ? TwoFactorAuthenticationStep : ReviewAndConfirmStep;
            return;
        }

        if (CurrentStep == TwoFactorAuthenticationStep)
        {
            if (!Is2FaConfirmed)
            {
                ErrorMessage = "You must confirm the 2FA step.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TwoFaToken)) TwoFaToken = GenerateTwoFaToken();

            CurrentStep = ReviewAndConfirmStep;
        }
    }

    internal void ExecuteBack()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep > SelectBillerStep)
        {
            if (CurrentStep == ReviewAndConfirmStep && Requires2Fa)
                CurrentStep = TwoFactorAuthenticationStep;
            else if (CurrentStep == ReviewAndConfirmStep && !Requires2Fa)
                CurrentStep = PaymentDetailsStep;
            else
                CurrentStep--;
        }
    }

    internal async Task ExecutePayBillAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (SelectedBiller == null)
            {
                ErrorMessage = "Please select a biller.";
                return;
            }

            if (string.IsNullOrWhiteSpace(BillerReference))
            {
                ErrorMessage = "Please enter a biller reference.";
                return;
            }

            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select a source account.";
                return;
            }

            if (Amount <= MinimumAmount)
            {
                ErrorMessage = "Please enter a valid amount.";
                return;
            }

            var request = new BillPayRequest
            {
                SourceAccountId = SelectedAccount.Id,
                BillerId = SelectedBiller.Id,
                BillerReference = BillerReference,
                Amount = Amount,
                IsPayInFull = false,
                TwoFaToken = Requires2Fa ? TwoFaToken : null,
            };

            ErrorOr<BillPayResponse> payResult = await _billPaymentClientService.PayBillAsync(request);

            if (payResult.IsError)
            {
                ErrorMessage = $"Payment failed: {payResult.FirstError.Description}";
                return;
            }

            if (ShouldSaveBiller)
            {
                var alreadySaved = SavedBillers.Any(savedBiller =>
                    savedBiller.BillerId == SelectedBiller.Id &&
                    string.Equals(savedBiller.DefaultReference, BillerReference, StringComparison.OrdinalIgnoreCase));

                if (!alreadySaved)
                {
                    var saveRequest = new SaveBillerRequest
                    {
                        BillerId = SelectedBiller.Id,
                        Nickname = SelectedBiller.Name,
                        DefaultReference = BillerReference,
                    };

                    ErrorOr<SavedBillerDto> saveResult = await _billPaymentClientService.SaveBillerAsync(saveRequest);

                    if (!saveResult.IsError) SavedBillers.Add(saveResult.Value);
                }
            }

            ReceiptNumber = payResult.Value.ReceiptNumber;
            Fee = payResult.Value.Fee;
            CurrentStep = PaymentResultStep;
        }
        catch (Exception paymentException)
        {
            ErrorMessage = $"Payment failed: {paymentException.Message}";
        }
    }

    internal void ResetForm()
    {
        ErrorMessage = string.Empty;
        ResetFormStateOnly();
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string GenerateTwoFaToken()
    {
        var random = new Random();
        return random.Next(MinimumTwoFactorToken, MaximumTwoFactorTokenExclusive)
            .ToString(CultureInfo.InvariantCulture);
    }

    private void ResetFormStateOnly()
    {
        CurrentStep = SelectBillerStep;
        SelectedBiller = null;
        SearchQuery = string.Empty;
        SelectedCategory = null;
        BillerReference = string.Empty;
        Amount = MinimumAmount;
        Fee = NoFee;
        ReceiptNumber = string.Empty;
        SelectedAccount = null;
        IsPayInFull = false;
        ShouldSaveBiller = false;
        Requires2Fa = false;
        Is2FaConfirmed = false;
        TwoFaToken = string.Empty;
    }

    private void ApplySavedDefaultsForSelectedBiller()
    {
        if (SelectedBiller == null || SavedBillers == null || SavedBillers.Count == MinimumBillers) return;

        SavedBillerDto? matchingSaved = SavedBillers.FirstOrDefault(s => s.BillerId == SelectedBiller.Id);

        if (matchingSaved != null &&
            string.IsNullOrWhiteSpace(BillerReference) &&
            !string.IsNullOrWhiteSpace(matchingSaved.DefaultReference))
            BillerReference = matchingSaved.DefaultReference!;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
