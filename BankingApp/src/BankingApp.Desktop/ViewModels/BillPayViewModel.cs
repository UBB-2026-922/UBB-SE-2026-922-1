namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Application.Features.BillPayments.Dtos;
using BankingApp.Application.Features.Billers.Dtos;
using Master;
using BankingApp.Desktop.Services;
using Views;
using ErrorOr;

/// <summary>Coordinates the multistep bill payment workflow in the desktop client.</summary>
public partial class BillPayViewModel : ObservableObject
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

    /// <summary>Initializes a new instance of the <see cref="BillPayViewModel"/> class.</summary>
    public BillPayViewModel(IBillPaymentClientService billPaymentClientService, IAppNavigationService navigationService)
    {
        _billPaymentClientService = billPaymentClientService;
        _navigationService = navigationService;

        Billers = new ObservableCollection<BillerDto>();
        SavedBillers = new ObservableCollection<SavedBillerDto>();
        Accounts = new ObservableCollection<AccountDto>();
        CurrentStep = SelectBillerStep;

        SearchCommand = new RelayCommand(ExecuteSearch);
        SelectBillerCommand = new RelayCommand<object?>(ExecuteSelectBiller);
        NextStepCommand = new RelayCommand(ExecuteNextStep);
        BackCommand = new RelayCommand(ExecuteBack);
        PayAnotherBillCommand = new RelayCommand(ResetForm);
        PayBillCommand = new AsyncRelayCommand(ExecutePayBillAsync);
        CancelCommand = new RelayCommand(() => _navigationService.NavigateToContent<DashboardView>());
    }

    /// <summary>Gets the command that refreshes billers using the current filters.</summary>
    public ICommand SearchCommand { get; }

    /// <summary>Gets the command that selects a biller or saved biller.</summary>
    public ICommand SelectBillerCommand { get; }

    /// <summary>Gets the command that advances the payment wizard.</summary>
    public ICommand NextStepCommand { get; }

    /// <summary>Gets the command that returns to the previous wizard step.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Gets the command that resets the flow after a completed payment.</summary>
    public ICommand PayAnotherBillCommand { get; }

    /// <summary>Gets the command that submits the current payment.</summary>
    public ICommand PayBillCommand { get; }

    /// <summary>Gets the command that abandons the bill payment flow.</summary>
    public ICommand CancelCommand { get; }

    /// <summary>Gets or sets the current wizard step.</summary>
    [ObservableProperty]
    public partial int CurrentStep { get; set; } = default!;

    /// <summary>Gets or sets the available billers.</summary>
    [ObservableProperty]
    public partial ObservableCollection<BillerDto> Billers { get; set; } = null!;

    /// <summary>Gets or sets the saved billers for the current user.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSavedBillers))]
public partial ObservableCollection<SavedBillerDto> SavedBillers { get; set; } = null!;

    /// <summary>Gets or sets the source accounts available for payment.</summary>
    [ObservableProperty]
    public partial ObservableCollection<AccountDto> Accounts { get; set; } = default!;

    /// <summary>Gets or sets the currently selected biller.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedBillerName))]
public partial BillerDto? SelectedBiller { get; set; } = default!;

    partial void OnSelectedBillerChanged(BillerDto? value)
    {
        ApplySavedDefaultsForSelectedBiller();
    }

    /// <summary>Gets or sets the biller search text.</summary>
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    /// <summary>Gets or sets the biller category filter.</summary>
    [ObservableProperty]
    public partial string? SelectedCategory { get; set; } = default!;

    partial void OnSelectedCategoryChanged(string? value)
    {
        ExecuteSearch();
    }

    /// <summary>Gets or sets the customer reference used by the selected biller.</summary>
    [ObservableProperty]
    public partial string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the bill amount.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReviewAmountText))]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(TotalText))]
    [NotifyPropertyChangedFor(nameof(AmountAsDouble))]
public partial decimal Amount { get; set; } = default!;

    /// <summary>Gets or sets the bill amount as a <see cref="double"/> for XAML bindings.</summary>
    public double AmountAsDouble
    {
        get => (double)Amount;
        set => Amount = (decimal)value;
    }

    /// <summary>Gets or sets a value indicating whether the payment should settle the full balance.</summary>
    [ObservableProperty]
    public partial bool IsPayInFull { get; set; } = default!;

    /// <summary>Gets or sets the account used to fund the payment.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedAccount { get; set; } = default!;

    /// <summary>Gets or sets the calculated payment fee.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReviewFeeText))]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(TotalText))]
public partial decimal Fee { get; set; } = default!;

    /// <summary>Gets or sets the receipt number returned after a successful payment.</summary>
    [ObservableProperty]
    public partial string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the current user-facing error message.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether two-factor confirmation is required.</summary>
    [ObservableProperty]
    public partial bool Requires2Fa { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether the user confirmed the two-factor step.</summary>
    [ObservableProperty]
    public partial bool Is2FaConfirmed { get; set; } = default!;

    /// <summary>Gets or sets the two-factor token entered for the payment.</summary>
    [ObservableProperty]
    public partial string TwoFaToken { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the selected biller should be saved for reuse.</summary>
    [ObservableProperty]
    public partial bool ShouldSaveBiller { get; set; } = default!;

    /// <summary>Gets a value indicating whether any saved billers are available.</summary>
    public bool HasSavedBillers => SavedBillers.Count > MinimumBillers;

    /// <summary>Gets the selected biller display name.</summary>
    public string SelectedBillerName => SelectedBiller?.Name ?? "No biller selected";

    /// <summary>Gets the formatted amount shown on the review step.</summary>
    public string ReviewAmountText =>
        Amount > MinimumAmount ? $"{Amount:0.00} RON" : "No amount entered";

    /// <summary>Gets the formatted fee shown on the review step.</summary>
    public string ReviewFeeText => $"{Fee:0.00} RON";

    /// <summary>Gets the payment total including fees.</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets the formatted payment total shown on the review step.</summary>
    public string TotalText => $"{Total:0.00} RON";

    /// <summary>Loads billers, saved billers, and source accounts for the workflow.</summary>
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

            Task<ErrorOr<List<BillerDto>>> task = _billPaymentClientService.GetBillersAsync(SearchQuery, SelectedCategory);
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

        SavedBillerDto? matchingSaved = SavedBillers.FirstOrDefault(s => s.BillerId == SelectedBiller.Id);

        if (matchingSaved != null &&
            string.IsNullOrWhiteSpace(BillerReference) &&
            !string.IsNullOrWhiteSpace(matchingSaved.DefaultReference))
        {
            BillerReference = matchingSaved.DefaultReference!;
        }
    }
}
