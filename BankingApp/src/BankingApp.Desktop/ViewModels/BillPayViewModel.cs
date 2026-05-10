namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.DTOs.BillPayments;
using Application.DTOs.Billers;
using Commands;
using Master;
using Services;
using Views;
using ErrorOr;
using Microsoft.UI.Xaml;

/// <summary>
/// Coordinates the multistep bill payment workflow in the Desktop client.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BillPayViewModel"/> class.
    /// </summary>
    /// <param name="billPaymentClientService">Provides biller, account, and payment operations.</param>
    /// <param name="navigationService">Handles page navigation within the desktop shell.</param>
    public BillPayViewModel(IBillPaymentClientService billPaymentClientService, IAppNavigationService navigationService)
    {
        _billPaymentClientService = billPaymentClientService;
        _navigationService = navigationService;

        _billers = new ObservableCollection<BillerDto>();
        _savedBillers = new ObservableCollection<SavedBillerDto>();
        _accounts = new ObservableCollection<AccountDto>();
        _currentStep = SelectBillerStep;

        SearchCommand = new RelayCommand(_ => ExecuteSearch());
        SelectBillerCommand = new RelayCommand(ExecuteSelectBiller);
        NextStepCommand = new RelayCommand(_ => ExecuteNextStep());
        BackCommand = new RelayCommand(_ => ExecuteBack());
        PayAnotherBillCommand = new RelayCommand(_ => ResetForm());
        PayBillCommand = new AsyncRelayCommand(_ => ExecutePayBillAsync());
        CancelCommand = new RelayCommand(_ =>
            _navigationService.NavigateToContent<DashboardView>());
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the current wizard step.
    /// </summary>
    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    /// <summary>
    /// Gets or sets the available billers.
    /// </summary>
    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        private set => SetProperty(ref _billers, value);
    }

    /// <summary>
    /// Gets or sets the saved billers for the current user.
    /// </summary>
    public ObservableCollection<SavedBillerDto> SavedBillers
    {
        get => _savedBillers;
        private set
        {
            if (!SetProperty(ref _savedBillers, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasSavedBillers));
            OnPropertyChanged(nameof(SavedBillersVisibility));
        }
    }

    /// <summary>
    /// Gets or sets the source accounts available for payment.
    /// </summary>
    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        private set => SetProperty(ref _accounts, value);
    }

    /// <summary>
    /// Gets or sets the currently selected biller.
    /// </summary>
    public BillerDto? SelectedBiller
    {
        get => _selectedBiller;
        private set
        {
            if (!SetProperty(ref _selectedBiller, value))
            {
                return;
            }

            ApplySavedDefaultsForSelectedBiller();
            OnPropertyChanged(nameof(SelectedBillerName));
        }
    }

    /// <summary>
    /// Gets or sets the biller search text.
    /// </summary>
    private string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    /// <summary>
    /// Gets or sets the biller category filter.
    /// </summary>
    private string? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                ExecuteSearch();
            }
        }
    }

    /// <summary>
    /// Gets or sets the customer reference used by the selected biller.
    /// </summary>
    public string BillerReference
    {
        get => _billerReference;
        set => SetProperty(ref _billerReference, value);
    }

    /// <summary>
    /// Gets or sets the bill amount.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the bill amount as a <see cref="double"/> for XAML bindings.
    /// </summary>
    public double AmountAsDouble
    {
        get => (double)_amount;
        set => Amount = (decimal)value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the payment should settle the full balance.
    /// </summary>
    public bool IsPayInFull
    {
        get => _isPayInFull;
        set => SetProperty(ref _isPayInFull, value);
    }

    /// <summary>
    /// Gets or sets the account used to fund the payment.
    /// </summary>
    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    /// <summary>
    /// Gets or sets the calculated payment fee.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the receipt number returned after a successful payment.
    /// </summary>
    public string ReceiptNumber
    {
        get => _receiptNumber;
        set => SetProperty(ref _receiptNumber, value);
    }

    /// <summary>
    /// Gets or sets the current user-facing error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor confirmation is required.
    /// </summary>
    public bool Requires2Fa
    {
        get => _requires2Fa;
        set => SetProperty(ref _requires2Fa, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user confirmed the two-factor step.
    /// </summary>
    public bool Is2FaConfirmed
    {
        get => _is2FaConfirmed;
        set => SetProperty(ref _is2FaConfirmed, value);
    }

    /// <summary>
    /// Gets or sets the two-factor token entered for the payment.
    /// </summary>
    private string TwoFaToken
    {
        get => _twoFaToken;
        set => SetProperty(ref _twoFaToken, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the selected biller should be saved for reuse.
    /// </summary>
    public bool ShouldSaveBiller
    {
        get => _shouldSaveBiller;
        set => SetProperty(ref _shouldSaveBiller, value);
    }

    /// <summary>
    /// Gets a value indicating whether any saved billers are available.
    /// </summary>
    public bool HasSavedBillers => SavedBillers.Count > MinimumBillers;

    /// <summary>
    /// Gets the visibility of the saved billers section.
    /// </summary>
    public Visibility SavedBillersVisibility =>
        HasSavedBillers ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Gets the visibility of the error message section.
    /// </summary>
    public Visibility ErrorMessageVisibility =>
        string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the selected biller display name.
    /// </summary>
    public string SelectedBillerName =>
        SelectedBiller?.Name ?? "No biller selected";

    /// <summary>
    /// Gets the formatted amount shown on the review step.
    /// </summary>
    public string ReviewAmountText =>
        Amount > MinimumAmount ? $"{Amount:0.00} RON" : "No amount entered";

    /// <summary>
    /// Gets the formatted fee shown on the review step.
    /// </summary>
    public string ReviewFeeText => $"{Fee:0.00} RON";

    /// <summary>
    /// Gets the payment total including fees.
    /// </summary>
    public decimal Total => Amount + Fee;

    /// <summary>
    /// Gets the formatted payment total shown on the review step.
    /// </summary>
    public string TotalText => $"{Total:0.00} RON";

    /// <summary>
    /// Gets the command that refreshes billers using the current filters.
    /// </summary>
    public ICommand SearchCommand { get; }

    /// <summary>
    /// Gets the command that selects a biller or saved biller.
    /// </summary>
    public ICommand SelectBillerCommand { get; }

    /// <summary>
    /// Gets the command that advances the payment wizard.
    /// </summary>
    public ICommand NextStepCommand { get; }

    /// <summary>
    /// Gets the command that returns to the previous wizard step.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Gets the command that resets the flow after a completed payment.
    /// </summary>
    public ICommand PayAnotherBillCommand { get; }

    /// <summary>
    /// Gets the command that submits the current payment.
    /// </summary>
    public ICommand PayBillCommand { get; }

    /// <summary>
    /// Gets the command that abandons the bill payment flow.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Loads billers, saved billers, and source accounts for the workflow.
    /// </summary>
    /// <returns>A task that completes when the data has been loaded.</returns>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ResetFormStateOnly();

            ErrorOr<List<BillerDto>> billersResult = await _billPaymentClientService.GetBillersAsync();
            if (!billersResult.IsError)
            {
                Billers = new ObservableCollection<BillerDto>(billersResult.Value);
            }

            ErrorOr<List<SavedBillerDto>> savedResult = await _billPaymentClientService.GetSavedBillersAsync();
            if (!savedResult.IsError)
            {
                SavedBillers = new ObservableCollection<SavedBillerDto>(savedResult.Value);
            }

            ErrorOr<List<AccountDto>> accountsResult = await _billPaymentClientService.GetAccountsAsync();
            if (!accountsResult.IsError)
            {
                Accounts = new ObservableCollection<AccountDto>(accountsResult.Value);
            }
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    private void ExecuteSearch()
    {
        try
        {
            ErrorMessage = string.Empty;
            string query = SearchQuery;

            Task<ErrorOr<List<BillerDto>>> task = _billPaymentClientService.GetBillersAsync(query, SelectedCategory);
            task.ContinueWith(
                completedTask =>
                {
                    if (!completedTask.Result.IsError)
                    {
                        Billers = new ObservableCollection<BillerDto>(completedTask.Result.Value);
                    }
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
            {
                BillerReference = savedBiller.DefaultReference!;
            }

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
            Requires2Fa = twoFaResult is { IsError: false, Value.Required: true };

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

            if (string.IsNullOrWhiteSpace(TwoFaToken))
            {
                TwoFaToken = GenerateTwoFaToken();
            }

            CurrentStep = ReviewAndConfirmStep;
        }
    }

    internal void ExecuteBack()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep > SelectBillerStep)
        {
            if (CurrentStep == ReviewAndConfirmStep && Requires2Fa)
            {
                CurrentStep = TwoFactorAuthenticationStep;
            }
            else if (CurrentStep == ReviewAndConfirmStep && !Requires2Fa)
            {
                CurrentStep = PaymentDetailsStep;
            }
            else
            {
                CurrentStep--;
            }
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
                bool alreadySaved = SavedBillers.Any(savedBiller =>
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

                    if (!saveResult.IsError)
                    {
                        SavedBillers.Add(saveResult.Value);
                    }
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

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string GenerateTwoFaToken()
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
        if (SelectedBiller == null || SavedBillers.Count == MinimumBillers)
        {
            return;
        }

        SavedBillerDto? matchingSaved = SavedBillers.FirstOrDefault(savedBiller => savedBiller.BillerId == SelectedBiller.Id);

        if (matchingSaved != null &&
            string.IsNullOrWhiteSpace(BillerReference) &&
            !string.IsNullOrWhiteSpace(matchingSaved.DefaultReference))
        {
            BillerReference = matchingSaved.DefaultReference!;
        }
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
