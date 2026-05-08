namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using BankingApp.Application.Features.Transfers.Dtos;
using Services.Transfers;
using BankingApp.Application.Common.Utilities;
using ErrorOr;

/// <summary>Drives the multistep transfer wizard.</summary>
public partial class TransferViewModel : ObservableObject
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
    private readonly ITransferClientService _transferClientService;

    /// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
    public TransferViewModel(ITransferClientService transferClientService)
    {
        _transferClientService = transferClientService ?? throw new ArgumentNullException(nameof(transferClientService));
        Accounts = new ObservableCollection<TransferAccountSelectionResponse>();
        CurrentStep = AccountSelectionStep;
        Currency = DefaultTransferCurrency;

        NextStepCommand = new RelayCommand(ExecuteNextStep);
        TransferCommand = new AsyncRelayCommand(ExecuteTransferAsync);
        CancelCommand = new RelayCommand(ExecuteCancel);
        SendAgainCommand = new RelayCommand(ExecuteSendAgain);
    }

    /// <summary>Gets the command that advances the wizard to the next step.</summary>
    public IRelayCommand NextStepCommand { get; }

    /// <summary>Gets the command that submits the transfer for processing.</summary>
    public IAsyncRelayCommand TransferCommand { get; }

    /// <summary>Gets the command that cancels the current transfer.</summary>
    public IRelayCommand CancelCommand { get; }

    /// <summary>Gets the command that resets the wizard for a new transfer.</summary>
    public IRelayCommand SendAgainCommand { get; }

    /// <summary>Gets the display name of the selected source account.</summary>
    public string SelectedAccountName => SelectedAccount?.AccountName ?? string.Empty;

    /// <summary>Gets or sets the current wizard step number.</summary>
    [ObservableProperty]
    public partial int CurrentStep { get; set; } = default!;

    /// <summary>Gets or sets the collection of accounts available for transfer.</summary>
    [ObservableProperty]
    public partial ObservableCollection<TransferAccountSelectionResponse> Accounts { get; set; } = default!;

    /// <summary>Gets or sets the account selected as the source for the transfer.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedAccountName))]
public partial TransferAccountSelectionResponse? SelectedAccount { get; set; } = default!;

    partial void OnSelectedAccountChanged(TransferAccountSelectionResponse? value)
    {
        _ = UpdateFxPreviewAsync();
    }

    /// <summary>Gets or sets the recipient display name.</summary>
    [ObservableProperty]
    public partial string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient IBAN.</summary>
    [ObservableProperty]
    public partial string RecipientIban { get; set; } = string.Empty;

    partial void OnRecipientIbanChanged(string value)
    {
        _ = UpdateIbanValidationAsync(value);
    }

    /// <summary>Gets or sets a value indicating whether the current IBAN passes validation.</summary>
    [ObservableProperty]
    public partial bool IsIbanValid { get; set; } = default!;

    /// <summary>Gets or sets the bank name inferred from the IBAN.</summary>
    [ObservableProperty]
    public partial string BankName { get; set; } = string.Empty;

    /// <summary>Gets or sets the parsed transfer amount.</summary>
    [ObservableProperty]
    public partial decimal Amount { get; set; } = default!;

    partial void OnAmountChanged(decimal value)
    {
        _ = UpdateFxPreviewAsync();
        UpdateRequires2Fa();
    }

    /// <summary>Gets or sets the target currency for the transfer.</summary>
    [ObservableProperty]
    public partial string Currency { get; set; } = default!;

    partial void OnCurrencyChanged(string value)
    {
        _ = UpdateFxPreviewAsync();
    }

    /// <summary>Gets or sets the human-readable FX preview text shown on the amount step.</summary>
    [ObservableProperty]
    public partial string FxPreviewText { get; set; } = string.Empty;

    /// <summary>Gets or sets the 2FA token generated for the transfer.</summary>
    [ObservableProperty]
    public partial string TwoFaToken { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the transfer amount requires two-factor authentication.</summary>
    [ObservableProperty]
    public partial bool Requires2Fa { get; set; } = default!;

    /// <summary>Gets or sets a value indicating whether the user has confirmed the 2FA step.</summary>
    [ObservableProperty]
    public partial bool Is2FaConfirmed { get; set; } = default!;

    /// <summary>Gets or sets the transaction reference returned after a successful transfer.</summary>
    [ObservableProperty]
    public partial string TransactionRef { get; set; } = string.Empty;

    /// <summary>Gets or sets the current error message displayed to the user.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether there is an active error message.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Gets or sets the raw text entered by the user for the transfer amount.</summary>
    [ObservableProperty]
    public partial string AmountText { get; set; } = string.Empty;

    partial void OnAmountTextChanged(string value)
    {
        Amount = decimal.TryParse(value, out decimal parsed) ? parsed : default;
    }

    /// <summary>Loads the authenticated user's accounts from the API and pre-selects the first account.</summary>
    public async Task LoadAccountsAsync()
    {
        try
        {
            ErrorOr<List<TransferAccountSelectionResponse>> result =
                await _transferClientService.GetAccountsAsync();

            if (result.IsError)
            {
                ErrorMessage = UserMessages.Transfer.AccountLoadFailed;
                return;
            }

            Accounts.Clear();
            foreach (TransferAccountSelectionResponse account in result.Value)
            {
                Accounts.Add(account);
            }

            if (Accounts.Count > MinimumAccounts)
            {
                SelectedAccount = Accounts[FirstAccountIndex];
            }
        }
        catch (Exception loadAccountsException)
        {
            ErrorMessage = loadAccountsException.Message;
        }
    }

    /// <summary>Generates a random six-digit 2FA token string for display to the user.</summary>
    internal static string GenerateTwoFaToken()
    {
        var random = new Random();
        return random.Next(MinimumTwoFactorToken, MaximumTwoFactorTokenExclusive)
            .ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Advances the wizard to the next step, validating IBAN, amount, and 2FA before allowing progression.</summary>
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

            if (Requires2Fa && string.IsNullOrWhiteSpace(TwoFaToken))
            {
                TwoFaToken = GenerateTwoFaToken();
            }

            CurrentStep = ReviewAndConfirmationStep;
            return;
        }

        CurrentStep++;
    }

    /// <summary>Submits the transfer to the API; on success advances to the completion step, on failure sets the error step.</summary>
    internal async Task ExecuteTransferAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (SelectedAccount == null)
            {
                throw new InvalidOperationException(UserMessages.Transfer.NoAccountSelected);
            }

            ErrorOr<TransferExecutionResponse> result =
                await _transferClientService.ExecuteTransferAsync(
                    SelectedAccount.Id,
                    RecipientName,
                    RecipientIban,
                    Amount,
                    Currency,
                    Requires2Fa ? TwoFaToken : null);

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

    /// <summary>Resets all form fields and returns the wizard to step 1.</summary>
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

    private void ExecuteCancel()
    {
        // TODO: implement.
        throw new NotImplementedException();
    }

    private async Task UpdateIbanValidationAsync(string iban)
    {
        try
        {
            ErrorOr<TransferIbanValidationResponse> result = await _transferClientService.ValidateIbanAsync(iban);

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

    private async Task UpdateFxPreviewAsync()
    {
        try
        {
            if (SelectedAccount == null || Amount <= ZeroAmount || string.IsNullOrWhiteSpace(Currency))
            {
                FxPreviewText = string.Empty;
                return;
            }

            ErrorOr<TransferForexPreviewResponse> result =
                await _transferClientService.GetFxPreviewAsync(SelectedAccount.Currency, Currency, Amount);

            if (result.IsError)
            {
                FxPreviewText = string.Empty;
                return;
            }

            TransferForexPreviewResponse preview = result.Value;

            FxPreviewText = preview.ExchangeRate == IdentityExchangeRate
                ? $"{Amount:F2} {Currency}"
                : $"{Amount:F2} {SelectedAccount.Currency} -> {preview.ConvertedAmount:F2} {Currency} (rate: {preview.ExchangeRate:F4})";
        }
        catch
        {
            FxPreviewText = string.Empty;
        }
    }

    private void UpdateRequires2Fa()
    {
        Requires2Fa = Amount >= TwoFaAmountThreshold;
    }
}
