namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BankingApp.Application.Features.BillPayments.Dtos;
using BankingApp.Application.Features.Billers.Dtos;
using BankingApp.Application.Features.RecurringPayments.Dtos;
using BankingApp.Desktop.Services;
using BankingApp.Domain.Enums;

/// <summary>Manages recurring bill payment creation and lifecycle actions for the desktop client.</summary>
public partial class RecurringPaymentViewModel : ObservableObject
{
    private const int NoBillerSelected = 0;
    private const decimal NoAmount = 0m;

    private readonly IBillPaymentClientService _billPaymentClientService;

    /// <summary>Initializes a new instance of the <see cref="RecurringPaymentViewModel"/> class.</summary>
    public RecurringPaymentViewModel(IBillPaymentClientService billPaymentClientService)
    {
        _billPaymentClientService = billPaymentClientService ?? throw new ArgumentNullException(nameof(billPaymentClientService));

        Payments = [];
        Accounts = [];
        Billers = [];
        Frequencies =
        [
            RecurringFrequency.Weekly,
            RecurringFrequency.Monthly,
            RecurringFrequency.Quarterly,
        ];

        Frequency = RecurringFrequency.Weekly;
        StartDate = DateTime.Today;

        CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
        PauseCommand = new AsyncRelayCommand<RecurringPaymentResponse?>(ExecutePauseAsync);
        ResumeCommand = new AsyncRelayCommand<RecurringPaymentResponse?>(ExecuteResumeAsync);
        CancelCommand = new AsyncRelayCommand<RecurringPaymentResponse?>(ExecuteCancelAsync);
    }

    /// <summary>Gets the command that creates a recurring payment.</summary>
    public IAsyncRelayCommand CreateCommand { get; }

    /// <summary>Gets the command that pauses an existing recurring payment.</summary>
    public IAsyncRelayCommand<RecurringPaymentResponse?> PauseCommand { get; }

    /// <summary>Gets the command that resumes an existing recurring payment.</summary>
    public IAsyncRelayCommand<RecurringPaymentResponse?> ResumeCommand { get; }

    /// <summary>Gets the command that cancels an existing recurring payment.</summary>
    public IAsyncRelayCommand<RecurringPaymentResponse?> CancelCommand { get; }

    /// <summary>Gets or sets the loaded recurring payments.</summary>
    [ObservableProperty]
    public partial ObservableCollection<RecurringPaymentResponse> Payments { get; set; } = default!;

    /// <summary>Gets or sets the currently selected recurring payment.</summary>
    [ObservableProperty]
    public partial RecurringPaymentResponse? SelectedPayment { get; set; } = default!;

    /// <summary>Gets or sets the selected biller identifier.</summary>
    [ObservableProperty]
    public partial int SelectedBillerId { get; set; } = default!;

    /// <summary>Gets or sets the recurring payment amount.</summary>
    [ObservableProperty]
    public partial decimal Amount { get; set; } = default!;

    /// <summary>Gets or sets the recurrence frequency.</summary>
    [ObservableProperty]
    public partial RecurringFrequency Frequency { get; set; } = default!;

    /// <summary>Gets or sets the first execution date.</summary>
    [ObservableProperty]
    public partial DateTime StartDate { get; set; } = default!;

    /// <summary>Gets or sets the optional final execution date.</summary>
    [ObservableProperty]
    public partial DateTime? EndDate { get; set; } = default!;

    /// <summary>Gets or sets the current user-facing error message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether an error message is currently available.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Gets or sets the accounts available as funding sources.</summary>
    [ObservableProperty]
    public partial ObservableCollection<AccountDto> Accounts { get; set; } = default!;

    /// <summary>Gets or sets the selected funding account.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedAccount { get; set; } = default!;

    /// <summary>Gets or sets the billers available for recurring payments.</summary>
    [ObservableProperty]
    public partial ObservableCollection<BillerDto> Billers { get; set; } = default!;

    /// <summary>Gets or sets the selected biller.</summary>
    [ObservableProperty]
    public partial BillerDto? SelectedBiller { get; set; } = default!;

    partial void OnSelectedBillerChanged(BillerDto? value)
    {
        SelectedBillerId = value?.Id ?? NoBillerSelected;
    }

    /// <summary>Gets or sets the supported recurrence frequencies.</summary>
    [ObservableProperty]
    public partial ObservableCollection<RecurringFrequency> Frequencies { get; set; } = default!;

    /// <summary>Creates a recurring payment using the current form values.</summary>
    public Task CreateAsync() => ExecuteCreateAsync();

    /// <summary>Pauses the specified recurring payment.</summary>
    public Task PauseAsync(RecurringPaymentResponse? payment) => ExecutePauseAsync(payment);

    /// <summary>Resumes the specified recurring payment.</summary>
    public Task ResumeAsync(RecurringPaymentResponse? payment) => ExecuteResumeAsync(payment);

    /// <summary>Cancels the specified recurring payment.</summary>
    public Task CancelAsync(RecurringPaymentResponse? payment) => ExecuteCancelAsync(payment);
}
