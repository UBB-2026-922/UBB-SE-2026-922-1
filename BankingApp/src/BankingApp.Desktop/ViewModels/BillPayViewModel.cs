// <copyright file="BillPayViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPayViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Application.DTOs.BillPayment;
using BankingApp.Desktop.Commands;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.Views;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Drives the multistep bill payment wizard.
///     Loads data via <see cref="IApiClient" /> and navigates via <see cref="IAppNavigationService" />.
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

    private readonly IApiClient _apiClient;
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
    ///     Initializes a new instance of the <see cref="BillPayViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client for server communication.</param>
    /// <param name="navigationService">The navigation service for page transitions.</param>
    public BillPayViewModel(IApiClient apiClient, IAppNavigationService navigationService)
    {
        _apiClient = apiClient;
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

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets or sets the current wizard step.</summary>
    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    /// <summary>Gets or sets the collection of billers from the directory.</summary>
    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        set => SetProperty(ref _billers, value);
    }

    /// <summary>Gets or sets the collection of saved billers.</summary>
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

    /// <summary>Gets or sets the collection of user accounts.</summary>
    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    /// <summary>Gets or sets the selected biller.</summary>
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

    /// <summary>Gets or sets the search query text.</summary>
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    /// <summary>Gets or sets the selected category filter.</summary>
    public string? SelectedCategory
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

    /// <summary>Gets or sets the biller reference (account number, contract ID).</summary>
    public string BillerReference
    {
        get => _billerReference;
        set => SetProperty(ref _billerReference, value);
    }

    /// <summary>Gets or sets the payment amount.</summary>
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

    /// <summary>Gets or sets the amount as a double for NumberBox binding.</summary>
    public double AmountAsDouble
    {
        get => (double)_amount;
        set => Amount = (decimal)value;
    }

    /// <summary>Gets or sets a value indicating whether to pay the full balance.</summary>
    public bool IsPayInFull
    {
        get => _isPayInFull;
        set => SetProperty(ref _isPayInFull, value);
    }

    /// <summary>Gets or sets the selected source account.</summary>
    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    /// <summary>Gets or sets the calculated fee.</summary>
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

    /// <summary>Gets or sets the receipt number after successful payment.</summary>
    public string ReceiptNumber
    {
        get => _receiptNumber;
        set => SetProperty(ref _receiptNumber, value);
    }

    /// <summary>Gets or sets the error message displayed to the user.</summary>
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

    /// <summary>Gets or sets a value indicating whether 2FA is required.</summary>
    public bool Requires2Fa
    {
        get => _requires2Fa;
        set => SetProperty(ref _requires2Fa, value);
    }

    /// <summary>Gets or sets a value indicating whether the user confirmed 2FA.</summary>
    public bool Is2FaConfirmed
    {
        get => _is2FaConfirmed;
        set => SetProperty(ref _is2FaConfirmed, value);
    }

    /// <summary>Gets or sets the generated 2FA token.</summary>
    public string TwoFaToken
    {
        get => _twoFaToken;
        set => SetProperty(ref _twoFaToken, value);
    }

    /// <summary>Gets or sets a value indicating whether to save the biller.</summary>
    public bool ShouldSaveBiller
    {
        get => _shouldSaveBiller;
        set => SetProperty(ref _shouldSaveBiller, value);
    }

    /// <summary>Gets a value indicating whether saved billers exist.</summary>
    public bool HasSavedBillers => SavedBillers != null && SavedBillers.Count > MinimumBillers;

    /// <summary>Gets the visibility of the saved billers section.</summary>
    public Visibility SavedBillersVisibility =>
        HasSavedBillers ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets the visibility of the error message.</summary>
    public Visibility ErrorMessageVisibility =>
        string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Gets the display name of the selected biller.</summary>
    public string SelectedBillerName =>
        SelectedBiller?.Name ?? "No biller selected";

    /// <summary>Gets the formatted amount text for the review step.</summary>
    public string ReviewAmountText =>
        Amount > MinimumAmount ? $"{Amount:0.00} RON" : "No amount entered";

    /// <summary>Gets the formatted fee text.</summary>
    public string ReviewFeeText => $"{Fee:0.00} RON";

    /// <summary>Gets the total payment amount (amount + fee).</summary>
    public decimal Total => Amount + Fee;

    /// <summary>Gets the formatted total text.</summary>
    public string TotalText => $"{Total:0.00} RON";

    /// <summary>Gets the search command.</summary>
    public ICommand SearchCommand { get; }

    /// <summary>Gets the select biller command.</summary>
    public ICommand SelectBillerCommand { get; }

    /// <summary>Gets the next step command.</summary>
    public ICommand NextStepCommand { get; }

    /// <summary>Gets the back command.</summary>
    public ICommand BackCommand { get; }

    /// <summary>Gets the pay another bill command.</summary>
    public ICommand PayAnotherBillCommand { get; }

    /// <summary>Gets the pay bill command.</summary>
    public ICommand PayBillCommand { get; }

    /// <summary>Gets the cancel command.</summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Loads billers, saved billers, and accounts from the API.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            ResetFormStateOnly();

            var billersResult = await _apiClient.GetAsync<List<BillerDto>>(ApiEndpoints.BillPayBillers);
            if (!billersResult.IsError)
            {
                Billers = new ObservableCollection<BillerDto>(billersResult.Value);
            }

            var savedResult = await _apiClient.GetAsync<List<SavedBillerDto>>(ApiEndpoints.BillPaySavedBillers);
            if (!savedResult.IsError)
            {
                SavedBillers = new ObservableCollection<SavedBillerDto>(savedResult.Value);
            }

            var accountsResult = await _apiClient.GetAsync<List<AccountDto>>(ApiEndpoints.BillPayAccounts);
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

    /// <summary>
    ///     Searches billers using the current query and category via the API.
    /// </summary>
    internal void ExecuteSearch()
    {
        try
        {
            ErrorMessage = string.Empty;
            string query = SearchQuery ?? string.Empty;
            string endpoint = $"{ApiEndpoints.BillPayBillersSearch}?search={Uri.EscapeDataString(query)}";
            if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                endpoint += $"&category={Uri.EscapeDataString(SelectedCategory)}";
            }

            var task = _apiClient.GetAsync<List<BillerDto>>(endpoint);
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

    /// <summary>
    ///     Sets the selected biller from a clicked Biller or SavedBiller and advances to the next step.
    /// </summary>
    /// <param name="parameter">The clicked item (BillerDto or SavedBillerDto).</param>
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

    /// <summary>
    ///     Validates current step inputs and advances the wizard.
    /// </summary>
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

            // Calculate fee and check 2FA via API
            var feeResult = _apiClient
                .GetAsync<FeeResponseDto>($"{ApiEndpoints.BillPayFee}?amount={Amount}")
                .GetAwaiter().GetResult();
            Fee = !feeResult.IsError ? feeResult.Value.Fee : 0m;

            var twoFaResult = _apiClient
                .GetAsync<Requires2FaResponseDto>($"{ApiEndpoints.BillPayRequires2Fa}?amount={Amount}")
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

            if (string.IsNullOrWhiteSpace(TwoFaToken))
            {
                TwoFaToken = GenerateTwoFaToken();
            }

            CurrentStep = ReviewAndConfirmStep;
        }
    }

    /// <summary>
    ///     Navigates back one step, skipping the 2FA step when it was not required.
    /// </summary>
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

    /// <summary>
    ///     Submits the bill payment via the API.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
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

            var request = new BillPayRequestDto
            {
                SourceAccountId = SelectedAccount.Id,
                BillerId = SelectedBiller.Id,
                BillerReference = BillerReference,
                Amount = Amount,
                IsPayInFull = false,
                TwoFaToken = Requires2Fa ? TwoFaToken : null,
            };

            var payResult = await _apiClient
                .PostAsync<BillPayRequestDto, BillPayResponseDto>(ApiEndpoints.BillPayPay, request);

            if (payResult.IsError)
            {
                ErrorMessage = $"Payment failed: {payResult.FirstError.Description}";
                return;
            }

            if (ShouldSaveBiller)
            {
                var alreadySaved = SavedBillers.Any(s =>
                    s.BillerId == SelectedBiller.Id &&
                    string.Equals(s.DefaultReference, BillerReference, StringComparison.OrdinalIgnoreCase));

                if (!alreadySaved)
                {
                    var saveRequest = new SaveBillerRequestDto
                    {
                        BillerId = SelectedBiller.Id,
                        Nickname = SelectedBiller.Name,
                        DefaultReference = BillerReference,
                    };

                    var saveResult = await _apiClient
                        .PostAsync<SaveBillerRequestDto, SavedBillerDto>(ApiEndpoints.BillPaySaveBiller, saveRequest);

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

    /// <summary>Clears the error message and resets the form.</summary>
    internal void ResetForm()
    {
        ErrorMessage = string.Empty;
        ResetFormStateOnly();
    }

    /// <summary>
    ///     Raises <see cref="PropertyChanged" /> for the given property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string GenerateTwoFaToken()
    {
        var random = new Random();
        return random.Next(MinimumTwoFactorToken, MaximumTwoFactorTokenExclusive).ToString();
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
        if (SelectedBiller == null || SavedBillers == null || SavedBillers.Count == MinimumBillers)
        {
            return;
        }

        var matchingSaved = SavedBillers.FirstOrDefault(s => s.BillerId == SelectedBiller.Id);

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
