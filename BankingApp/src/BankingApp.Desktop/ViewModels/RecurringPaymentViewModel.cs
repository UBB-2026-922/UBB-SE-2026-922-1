namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.DTOs.BillPayments;
using Application.DTOs.Billers;
using Application.DTOs.RecurringPayments;
using Services;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.UI.Xaml;

/// <summary>
/// Manages recurring bill payment creation and lifecycle actions for the desktop client.
/// </summary>
public partial class RecurringPaymentViewModel : INotifyPropertyChanged
{
    private const int NoBillerSelected = 0;
    private const decimal NoAmount = 0m;

    private readonly IBillPaymentClientService _billPaymentClientService;

    private ObservableCollection<RecurringPaymentResponse> _payments;
    private RecurringPaymentResponse? _selectedPayment;
    private int _selectedBillerId;
    private decimal _amount;
    private RecurringFrequency _frequency;
    private DateTime _startDate;
    private DateTime? _endDate;
    private string _errorMessage = string.Empty;

    private ObservableCollection<AccountDto> _accounts;
    private AccountDto? _selectedAccount;

    private ObservableCollection<BillerDto> _billers;
    private BillerDto? _selectedBiller;

    private ObservableCollection<RecurringFrequency> _frequencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringPaymentViewModel"/> class.
    /// </summary>
    /// <param name="billPaymentClientService">Provides recurring payment, biller, and account operations.</param>
    public RecurringPaymentViewModel(IBillPaymentClientService billPaymentClientService)
    {
        _billPaymentClientService = billPaymentClientService ?? throw new ArgumentNullException(nameof(billPaymentClientService));

        _payments = [];
        _accounts = [];
        _billers = [];
        _frequencies =
        [
            RecurringFrequency.Weekly,
            RecurringFrequency.Monthly,
            RecurringFrequency.Quarterly,
        ];

        _selectedPayment = null;
        _selectedBillerId = NoBillerSelected;
        _amount = NoAmount;
        _frequency = RecurringFrequency.Weekly;
        _startDate = DateTime.Today;
        _endDate = null;
        _selectedAccount = null;
        _selectedBiller = null;

        CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
        PauseCommand = new AsyncRelayCommand(ExecutePauseAsync);
        ResumeCommand = new AsyncRelayCommand(ExecuteResumeAsync);
        CancelCommand = new AsyncRelayCommand(ExecuteCancelAsync);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the loaded recurring payments.
    /// </summary>
    public ObservableCollection<RecurringPaymentResponse> Payments
    {
        get => _payments;
        private set => SetProperty(ref _payments, value);
    }

    /// <summary>
    /// Gets or sets the currently selected recurring payment.
    /// </summary>
    public RecurringPaymentResponse? SelectedPayment
    {
        get => _selectedPayment;
        set => SetProperty(ref _selectedPayment, value);
    }

    /// <summary>
    /// Gets or sets the selected biller identifier.
    /// </summary>
    public int SelectedBillerId
    {
        get => _selectedBillerId;
        set => SetProperty(ref _selectedBillerId, value);
    }

    /// <summary>
    /// Gets or sets the recurring payment amount.
    /// </summary>
    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    /// <summary>
    /// Gets or sets the recurrence frequency.
    /// </summary>
    public RecurringFrequency Frequency
    {
        get => _frequency;
        set => SetProperty(ref _frequency, value);
    }

    /// <summary>
    /// Gets or sets the first execution date.
    /// </summary>
    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    /// <summary>
    /// Gets or sets the optional final execution date.
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
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
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether an error message is currently available.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Gets the visibility of the error message area.
    /// </summary>
    public Visibility ErrorMessageVisibility =>
        HasError ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Gets or sets the accounts available as funding sources.
    /// </summary>
    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        private set => SetProperty(ref _accounts, value);
    }

    /// <summary>
    /// Gets or sets the selected funding account.
    /// </summary>
    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    /// <summary>
    /// Gets or sets the billers available for recurring payments.
    /// </summary>
    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        private set => SetProperty(ref _billers, value);
    }

    /// <summary>
    /// Gets or sets the selected biller.
    /// </summary>
    public BillerDto? SelectedBiller
    {
        get => _selectedBiller;
        set
        {
            if (SetProperty(ref _selectedBiller, value))
            {
                SelectedBillerId = value?.Id ?? NoBillerSelected;
            }
        }
    }

    /// <summary>
    /// Gets or sets the supported recurrence frequencies.
    /// </summary>
    public ObservableCollection<RecurringFrequency> Frequencies
    {
        get => _frequencies;
        set => SetProperty(ref _frequencies, value);
    }

    /// <summary>
    /// Gets the command that creates a recurring payment.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Gets the command that pauses an existing recurring payment.
    /// </summary>
    public ICommand PauseCommand { get; }

    /// <summary>
    /// Gets the command that resumes an existing recurring payment.
    /// </summary>
    public ICommand ResumeCommand { get; }

    /// <summary>
    /// Gets the command that cancels an existing recurring payment.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Loads the accounts, billers, and existing recurring payments for the current user.
    /// </summary>
    /// <returns>A task that completes when the data has been loaded.</returns>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            ErrorOr<List<AccountDto>> accountsResult = await _billPaymentClientService.GetAccountsAsync();
            if (!accountsResult.IsError)
            {
                Accounts = new ObservableCollection<AccountDto>(accountsResult.Value);
            }

            ErrorOr<List<RecurringPaymentResponse>> paymentsResult =
                await _billPaymentClientService.GetRecurringPaymentsAsync();
            if (!paymentsResult.IsError)
            {
                Payments = new ObservableCollection<RecurringPaymentResponse>(paymentsResult.Value);
            }

            ErrorOr<List<BillerDto>> billersResult = await _billPaymentClientService.GetBillersAsync();
            if (!billersResult.IsError)
            {
                Billers = new ObservableCollection<BillerDto>(billersResult.Value);
            }
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    /// <summary>
    /// Creates a recurring payment using the current form values.
    /// </summary>
    /// <returns>A task that completes when the create request finishes.</returns>
    public Task CreateAsync() => ExecuteCreateAsync();

    /// <summary>
    /// Pauses the specified recurring payment.
    /// </summary>
    /// <param name="payment">The recurring payment to pause.</param>
    /// <returns>A task that completes when the pause request finishes.</returns>
    public Task PauseAsync(RecurringPaymentResponse? payment) => ExecutePauseAsync(payment);

    /// <summary>
    /// Resumes the specified recurring payment.
    /// </summary>
    /// <param name="payment">The recurring payment to resume.</param>
    /// <returns>A task that completes when the resume request finishes.</returns>
    public Task ResumeAsync(RecurringPaymentResponse? payment) => ExecuteResumeAsync(payment);

    /// <summary>
    /// Cancels the specified recurring payment.
    /// </summary>
    /// <param name="payment">The recurring payment to cancel.</param>
    /// <returns>A task that completes when the cancel request finishes.</returns>
    public Task CancelAsync(RecurringPaymentResponse? payment) => ExecuteCancelAsync(payment);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task ExecuteCreateAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (SelectedBiller == null)
            {
                ErrorMessage = "Please select a biller.";
                return;
            }

            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select a source account.";
                return;
            }

            if (Amount <= NoAmount)
            {
                ErrorMessage = "Please enter a valid amount.";
                return;
            }

            if (EndDate.HasValue && EndDate.Value.Date < StartDate.Date)
            {
                ErrorMessage = "End date cannot be earlier than start date.";
                return;
            }

            var request = new CreateRecurringPaymentRequest
            {
                BillerId = SelectedBiller.Id,
                SourceAccountId = SelectedAccount.Id,
                Amount = Amount,
                IsPayInFull = false,
                Frequency = Frequency,
                StartDate = StartDate,
                EndDate = EndDate,
            };

            ErrorOr<RecurringPaymentResponse> result =
                await _billPaymentClientService.CreateRecurringPaymentAsync(request);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                return;
            }

            Payments.Add(result.Value);
            ClearForm();
        }
        catch (Exception createPaymentException)
        {
            ErrorMessage = $"Failed to create recurring payment: {createPaymentException.Message}";
        }
    }

    private async Task ExecutePauseAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentResponse payment)
            {
                ErrorMessage = "Please select a recurring payment to pause.";
                return;
            }

            ErrorOr<Success> result = await _billPaymentClientService.PauseRecurringPaymentAsync(payment.Id);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                return;
            }

            UpdatePaymentInCollection(payment.Id, RecurringPaymentStatus.Paused);
        }
        catch (Exception executePauseException)
        {
            ErrorMessage = $"Failed to pause recurring payment: {executePauseException.Message}";
        }
    }

    private async Task ExecuteResumeAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentResponse payment)
            {
                ErrorMessage = "Please select a recurring payment to resume.";
                return;
            }

            ErrorOr<Success> result = await _billPaymentClientService.ResumeRecurringPaymentAsync(payment.Id);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                return;
            }

            UpdatePaymentInCollection(payment.Id, RecurringPaymentStatus.Active);
        }
        catch (Exception executeResumeException)
        {
            ErrorMessage = $"Failed to resume recurring payment: {executeResumeException.Message}";
        }
    }

    private async Task ExecuteCancelAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentResponse payment)
            {
                ErrorMessage = "Please select a recurring payment to cancel.";
                return;
            }

            ErrorOr<Success> result = await _billPaymentClientService.CancelRecurringPaymentAsync(payment.Id);

            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                return;
            }

            UpdatePaymentInCollection(payment.Id, RecurringPaymentStatus.Cancelled);
        }
        catch (Exception executeCancelException)
        {
            ErrorMessage = $"Failed to cancel recurring payment: {executeCancelException.Message}";
        }
    }

    private void UpdatePaymentInCollection(int paymentId, RecurringPaymentStatus newStatus)
    {
        RecurringPaymentResponse? existingPayment = Payments.FirstOrDefault(payment => payment.Id == paymentId);
        if (existingPayment == null)
        {
            return;
        }

        int index = Payments.IndexOf(existingPayment);
        existingPayment.Status = newStatus;
        Payments[index] = existingPayment;
    }

    private void ClearForm()
    {
        SelectedPayment = null;
        SelectedBiller = null;
        SelectedBillerId = NoBillerSelected;
        SelectedAccount = null;
        Amount = NoAmount;
        Frequency = RecurringFrequency.Weekly;
        StartDate = DateTime.Today;
        EndDate = null;
        ErrorMessage = string.Empty;
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

    private sealed partial class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsyncWithParam;
        private readonly Func<Task>? _executeAsyncNoParam;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> executeAsync)
        {
            _executeAsyncNoParam = executeAsync;
            _executeAsyncWithParam = _ => Task.CompletedTask;
        }

        public AsyncRelayCommand(Func<object?, Task> executeAsync)
        {
            _executeAsyncWithParam = executeAsync;
            _executeAsyncNoParam = null;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => !_isExecuting;

        public void Execute(object? parameter)
        {
            _ = ExecuteAsync(parameter);
        }

        private async Task ExecuteAsync(object? parameter)
        {
            _isExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                if (_executeAsyncNoParam != null)
                {
                    await _executeAsyncNoParam();
                }
                else
                {
                    await _executeAsyncWithParam(parameter);
                }
            }
            finally
            {
                _isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
