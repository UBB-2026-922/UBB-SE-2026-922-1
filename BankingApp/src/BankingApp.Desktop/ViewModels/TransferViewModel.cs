namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;

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
    private readonly ITransferService _transferService;

    /// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
    public TransferViewModel(ITransferService transferService)
    {
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
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
    public partial decimal Amount { get; set; } = 0;

    partial void OnAmountChanged(decimal value)
    {
        _ = UpdateFxPreviewAsync();
        UpdateRequires2Fa();
    }

    /// <summary>Gets or sets the target currency for the transfer.</summary>
    [ObservableProperty]
    public partial string Currency { get; set; }

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
        Amount = decimal.TryParse(value, out decimal parsed) ? parsed : 0;
    }
}