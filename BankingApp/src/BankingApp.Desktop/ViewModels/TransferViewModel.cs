// <copyright file="TransferViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Desktop.Models;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Drives the multi-step transfer wizard.
///     Uses <see cref="IApiClient" /> for all server communication during the transfer flow.
/// </summary>
public partial class TransferViewModel : INotifyPropertyChanged
{
    private const int AccountSelectionStep = 1;
    private const int RecipientDetailsStep = 2;
    private const int AmountDetailsStep = 3;
    private const int TwoFactorAuthenticationStep = 4;
    private const int ReviewAndConfirmationStep = 5;
    private const int TransferCompletedStep = 6;
    private const int TransferErrorStep = 7;
    private const int MinimumTwoFactorToken = 100000;
    private const int MaximumTwoFactorTokenExclusive = 1000000;
    private const decimal ZeroAmount = 0m;
    private const decimal IdentityExchangeRate = 1m;
    private const decimal TwoFaAmountThreshold = 1000m;
    private const string DefaultTransferCurrency = "EUR";
    private const int MinimumAccounts = 0;
    private const int FirstAccountIndex = 0;
    private readonly IApiClient _apiClient;

    private int _currentStep;
    private ObservableCollection<TransferAccountDto> _accounts;
    private TransferAccountDto? _selectedAccount;
    private string _recipientName = string.Empty;
    private string _recipientIban = string.Empty;
    private bool _isIbanValid;
    private string _bankName = string.Empty;
    private decimal _amount;
    private string _currency = DefaultTransferCurrency;
    private string _fxPreviewText = string.Empty;
    private string _twoFaToken = string.Empty;
    private bool _requires2Fa;
    private bool _is2FaConfirmed;
    private string _transactionRef = string.Empty;
    private string _errorMessage = string.Empty;
    private string _amountText = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for all transfer-related server calls.</param>
    public TransferViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _accounts = new ObservableCollection<TransferAccountDto>();
        _currentStep = AccountSelectionStep;

        NextStepCommand = new RelayCommand(ExecuteNextStep);
        TransferCommand = new AsyncRelayCommand(ExecuteTransferAsync);
        CancelCommand = new RelayCommand(ExecuteCancel);
        SendAgainCommand = new RelayCommand(ExecuteSendAgain);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the display name of the selected source account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string SelectedAccountName => SelectedAccount?.AccountName ?? string.Empty;

    /// <summary>
    ///     Gets or sets the current wizard step number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    /// <summary>
    ///     Gets or sets the collection of accounts available for transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableCollection<TransferAccountDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    /// <summary>
    ///     Gets or sets the account selected as the source for the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferAccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            SetProperty(ref _selectedAccount, value);
            OnPropertyChanged(nameof(SelectedAccountName));
            _ = UpdateFxPreviewAsync();
        }
    }

    /// <summary>
    ///     Gets or sets the recipient display name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientName
    {
        get => _recipientName;
        set => SetProperty(ref _recipientName, value);
    }

    /// <summary>
    ///     Gets or sets the recipient IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientIban
    {
        get => _recipientIban;
        set
        {
            SetProperty(ref _recipientIban, value);
            _ = UpdateIbanValidationAsync(value);
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the current IBAN passes validation.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsIbanValid
    {
        get => _isIbanValid;
        set => SetProperty(ref _isIbanValid, value);
    }

    /// <summary>
    ///     Gets or sets the bank name inferred from the IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string BankName
    {
        get => _bankName;
        set => SetProperty(ref _bankName, value);
    }

    /// <summary>
    ///     Gets or sets the parsed transfer amount.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount
    {
        get => _amount;
        set
        {
            SetProperty(ref _amount, value);
            _ = UpdateFxPreviewAsync();
            UpdateRequires2Fa();
        }
    }

    /// <summary>
    ///     Gets or sets the target currency for the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency
    {
        get => _currency;
        set
        {
            SetProperty(ref _currency, value);
            _ = UpdateFxPreviewAsync();
        }
    }

    /// <summary>
    ///     Gets or sets the human-readable FX preview text shown on the amount step.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string FxPreviewText
    {
        get => _fxPreviewText;
        set => SetProperty(ref _fxPreviewText, value);
    }

    /// <summary>
    ///     Gets or sets the 2FA token generated for the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TwoFaToken
    {
        get => _twoFaToken;
        set => SetProperty(ref _twoFaToken, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the transfer amount requires two-factor authentication.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Requires2Fa
    {
        get => _requires2Fa;
        set => SetProperty(ref _requires2Fa, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the user has confirmed the 2FA step.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Is2FaConfirmed
    {
        get => _is2FaConfirmed;
        set => SetProperty(ref _is2FaConfirmed, value);
    }

    /// <summary>
    ///     Gets or sets the transaction reference returned after a successful transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TransactionRef
    {
        get => _transactionRef;
        set => SetProperty(ref _transactionRef, value);
    }

    /// <summary>
    ///     Gets or sets the current error message displayed to the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    ///     Gets a value indicating whether there is an active error message.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    ///     Gets or sets the raw text entered by the user for the transfer amount.
    ///     Automatically parses the value into <see cref="Amount" />.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AmountText
    {
        get => _amountText;
        set
        {
            SetProperty(ref _amountText, value);

            if (decimal.TryParse(value, out decimal parsed))
                Amount = parsed;
            else
                Amount = default;
        }
    }

    /// <summary>
    ///     Gets the command that advances the wizard to the next step.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ICommand NextStepCommand { get; }

    /// <summary>
    ///     Gets the command that submits the transfer for processing.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ICommand TransferCommand { get; }

    /// <summary>
    ///     Gets the command that cancels the current transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Gets the command that resets the wizard for a new transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ICommand SendAgainCommand { get; }

    /// <summary>
    ///     Loads the authenticated user's accounts from the API and pre-selects the first account.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task LoadAccountsAsync()
    {
        try
        {
            ErrorOr<List<TransferAccountDto>> result =
                await _apiClient.GetAsync<List<TransferAccountDto>>(ApiEndpoints.TransferAccounts);

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Transfer.AccountLoadFailed;
                return;
            }

            Accounts.Clear();

            foreach (TransferAccountDto account in result.Value) Accounts.Add(account);

            if (Accounts.Count > MinimumAccounts) SelectedAccount = Accounts[FirstAccountIndex];
        }
        catch (Exception loadAccountsException)
        {
            ErrorMessage = loadAccountsException.Message;
        }
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Generates a random six-digit 2FA token string for display to the user.
    /// </summary>
    /// <returns>A six-digit string token.</returns>
    internal string GenerateTwoFaToken()
    {
        var random = new Random();
        return random.Next(MinimumTwoFactorToken, MaximumTwoFactorTokenExclusive)
            .ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Advances the wizard to the next step, validating IBAN, amount,
    ///     and 2FA confirmation before allowing progression.
    /// </summary>
    internal void ExecuteNextStep()
    {
        ErrorMessage = string.Empty;

        if (CurrentStep == RecipientDetailsStep && !IsIbanValid)
        {
            ErrorMessage = UserMessages.Transfer.InvalidIban;
            CurrentStep = TransferErrorStep;
            return;
        }

        if (CurrentStep == AmountDetailsStep)
        {
            if (Amount <= ZeroAmount)
            {
                ErrorMessage = UserMessages.Transfer.AmountMustBePositive;
                CurrentStep = TransferErrorStep;
                return;
            }

            CurrentStep = Requires2Fa ? TwoFactorAuthenticationStep : ReviewAndConfirmationStep;
            return;
        }

        if (CurrentStep == TwoFactorAuthenticationStep)
        {
            if (!Is2FaConfirmed)
            {
                ErrorMessage = UserMessages.Transfer.TwoFaRequired;
                CurrentStep = TransferErrorStep;
                return;
            }

            if (Requires2Fa && string.IsNullOrWhiteSpace(TwoFaToken)) TwoFaToken = GenerateTwoFaToken();

            CurrentStep = ReviewAndConfirmationStep;
            return;
        }

        CurrentStep++;
    }

    /// <summary>
    ///     Submits the transfer to the API; on success advances to the completion step,
    ///     on failure sets the error step.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    internal async Task ExecuteTransferAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (SelectedAccount == null) throw new InvalidOperationException(UserMessages.Transfer.NoAccountSelected);

            var request = new TransferRequestDto
            {
                SourceAccountId = SelectedAccount.Id,
                RecipientName = RecipientName,
                RecipientIban = RecipientIban,
                Amount = Amount,
                Currency = Currency,
                TwoFaToken = Requires2Fa ? TwoFaToken : null
            };

            ErrorOr<TransferResultDto> result =
                await _apiClient.PostAsync<TransferRequestDto, TransferResultDto>(
                    ApiEndpoints.TransferExecute,
                    request);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                CurrentStep = TransferErrorStep;
                return;
            }

            TransactionRef = result.Value.TransactionRef;
            CurrentStep = TransferCompletedStep;
        }
        catch (Exception executeTransferException)
        {
            ErrorMessage = executeTransferException.Message;
            CurrentStep = TransferErrorStep;
        }
    }

    /// <summary>
    ///     Resets all form fields and returns the wizard to step 1 so the user can
    ///     initiate another transfer.
    /// </summary>
    internal void ExecuteSendAgain()
    {
        SelectedAccount = Accounts.Count > MinimumAccounts ? Accounts[FirstAccountIndex] : null;
        RecipientName = string.Empty;
        RecipientIban = string.Empty;
        IsIbanValid = false;
        BankName = string.Empty;
        Amount = ZeroAmount;
        Currency = DefaultTransferCurrency;
        FxPreviewText = string.Empty;
        TwoFaToken = string.Empty;
        Requires2Fa = false;
        Is2FaConfirmed = false;
        TransactionRef = string.Empty;
        ErrorMessage = string.Empty;
        AmountText = string.Empty;

        CurrentStep = AccountSelectionStep;
    }

    /// <summary>
    ///     Sets a field value and raises <see cref="PropertyChanged" /> when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name (auto-filled by the compiler).</param>
    /// <returns><see langword="true" /> if the value changed; otherwise <see langword="false" />.</returns>
    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void ExecuteCancel()
    {
    }

    /// <summary>
    ///     Validates the IBAN via the API and updates <see cref="IsIbanValid" /> and <see cref="BankName" />.
    /// </summary>
    /// <param name="iban">The IBAN to validate.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private async Task UpdateIbanValidationAsync(string iban)
    {
        try
        {
            ErrorOr<ValidateIbanResponse> result =
                await _apiClient.PostAsync<object, ValidateIbanResponse>(
                    ApiEndpoints.TransferValidateIban,
                    new { Iban = iban });

            if (result.IsError)
            {
                IsIbanValid = false;
                BankName = string.Empty;
                return;
            }

            IsIbanValid = result.Value.IsValid;
            BankName = result.Value.IsValid ? result.Value.BankName : string.Empty;
        }
        catch
        {
            IsIbanValid = false;
            BankName = string.Empty;
        }
    }

    /// <summary>
    ///     Recalculates and updates <see cref="FxPreviewText" /> to show the converted amount
    ///     and rate whenever the account, amount, or currency changes.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private async Task UpdateFxPreviewAsync()
    {
        try
        {
            if (SelectedAccount == null || Amount <= ZeroAmount || string.IsNullOrWhiteSpace(Currency))
            {
                FxPreviewText = string.Empty;
                return;
            }

            string endpoint =
                $"{ApiEndpoints.TransferFxPreview}?from={SelectedAccount.Currency}&to={Currency}&amount={Amount}";

            ErrorOr<FxPreviewDto> result = await _apiClient.GetAsync<FxPreviewDto>(endpoint);

            if (result.IsError)
            {
                FxPreviewText = string.Empty;
                return;
            }

            FxPreviewDto preview = result.Value;

            if (preview.ExchangeRate == IdentityExchangeRate)
                FxPreviewText = $"{Amount:F2} {Currency}";
            else
                FxPreviewText =
                    $"{Amount:F2} {SelectedAccount.Currency} -> {preview.ConvertedAmount:F2} {Currency} (rate: {preview.ExchangeRate:F4})";
        }
        catch
        {
            FxPreviewText = string.Empty;
        }
    }

    /// <summary>
    ///     Updates <see cref="Requires2Fa" /> based on whether the current amount
    ///     meets the 2FA threshold. Kept client-side for instant UX feedback.
    /// </summary>
    private void UpdateRequires2Fa()
    {
        Requires2Fa = Amount >= TwoFaAmountThreshold;
    }

    /// <summary>
    ///     Simple synchronous relay command for the transfer wizard buttons.
    /// </summary>
    private sealed partial class RelayCommand : ICommand
    {
        private readonly Action _execute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        /// <inheritdoc />
#pragma warning disable CS0067 // Required by ICommand but never raised by this simple implementation
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    /// <summary>
    ///     Asynchronous relay command that prevents re-entrant execution.
    /// </summary>
    private sealed partial class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private bool _isExecuting;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
        /// </summary>
        /// <param name="executeAsync">The asynchronous action to execute when the command is invoked.</param>
        public AsyncRelayCommand(Func<Task> executeAsync)
        {
            _executeAsync = executeAsync;
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting;
        }

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            _ = ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            _isExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                await _executeAsync();
            }
            finally
            {
                _isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
