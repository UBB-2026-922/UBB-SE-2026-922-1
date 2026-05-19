namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BankingApp.Domain.Enums;
using Contracts.Features.Billers.Dtos;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Billers.Services;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using Microsoft.UI.Xaml;

/// <summary>Manages recurring bill payment creation and lifecycle actions for the desktop client.</summary>
public partial class RecurringPaymentViewModel : ObservableObject
{
    private const int NoBillerSelected = 0;
    private const decimal NoAmount = 0m;

    private readonly IBillPaymentService _billPaymentService;
    private readonly IBillerService _billerService;
    private readonly IRecurringPaymentService _recurringPaymentService;

    /// <summary>Initializes a new instance of the <see cref="RecurringPaymentViewModel"/> class.</summary>
    public RecurringPaymentViewModel(
        IBillPaymentService billPaymentService,
        IBillerService billerService,
        IRecurringPaymentService recurringPaymentService)
    {
        _billPaymentService = billPaymentService ?? throw new ArgumentNullException(nameof(billPaymentService));
        _billerService = billerService ?? throw new ArgumentNullException(nameof(billerService));
        _recurringPaymentService = recurringPaymentService ?? throw new ArgumentNullException(nameof(recurringPaymentService));

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
    [NotifyPropertyChangedFor(nameof(ErrorMessageVisibility))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether an error message is currently available.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Gets the visibility of the error message panel.</summary>
    public Visibility ErrorMessageVisibility =>
        HasError ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets or sets the accounts available as funding sources.</summary>
    [ObservableProperty]
    public partial ObservableCollection<AccountDto> Accounts { get; set; }

    /// <summary>Gets or sets the selected funding account.</summary>
    [ObservableProperty]
    public partial AccountDto? SelectedAccount { get; set; } = default!;

    /// <summary>Gets or sets the billers available for recurring payments.</summary>
    [ObservableProperty]
    public partial ObservableCollection<BillerDto> Billers { get; set; }

    /// <summary>Gets or sets the selected biller.</summary>
    [ObservableProperty]
    public partial BillerDto? SelectedBiller { get; set; } = null!;

    partial void OnSelectedBillerChanged(BillerDto? value)
    {
        SelectedBillerId = value?.Id ?? NoBillerSelected;
    }

    /// <summary>Gets or sets the supported recurrence frequencies.</summary>
    [ObservableProperty]
    public partial ObservableCollection<RecurringFrequency> Frequencies { get; set; }

    /// <summary>Creates a recurring payment using the current form values.</summary>
    public Task CreateAsync() => ExecuteCreateAsync();

    /// <summary>Pauses the specified recurring payment.</summary>
    public Task PauseAsync(RecurringPaymentResponse? payment) => ExecutePauseAsync(payment);

    /// <summary>Resumes the specified recurring payment.</summary>
    public Task ResumeAsync(RecurringPaymentResponse? payment) => ExecuteResumeAsync(payment);

    /// <summary>Cancels the specified recurring payment.</summary>
    public Task CancelAsync(RecurringPaymentResponse? payment) => ExecuteCancelAsync(payment);
}
