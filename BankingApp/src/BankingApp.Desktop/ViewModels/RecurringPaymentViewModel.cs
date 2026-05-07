using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.DTOs.Billers;
using BankingApp.Application.DTOs.RecurringPayments;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.ViewModels;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<RecurringPaymentResponse> Payments
    {
        get => _payments;
        set => SetProperty(ref _payments, value);
    }

    public RecurringPaymentResponse? SelectedPayment
    {
        get => _selectedPayment;
        set => SetProperty(ref _selectedPayment, value);
    }

    public int SelectedBillerId
    {
        get => _selectedBillerId;
        set => SetProperty(ref _selectedBillerId, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public RecurringFrequency Frequency
    {
        get => _frequency;
        set => SetProperty(ref _frequency, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

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

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Visibility ErrorMessageVisibility =>
        HasError ? Visibility.Visible : Visibility.Collapsed;

    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        set => SetProperty(ref _billers, value);
    }

    public BillerDto? SelectedBiller
    {
        get => _selectedBiller;
        set
        {
            if (SetProperty(ref _selectedBiller, value)) SelectedBillerId = value?.Id ?? NoBillerSelected;
        }
    }

    public ObservableCollection<RecurringFrequency> Frequencies
    {
        get => _frequencies;
        set => SetProperty(ref _frequencies, value);
    }

    public ICommand CreateCommand { get; }

    public ICommand PauseCommand { get; }

    public ICommand ResumeCommand { get; }

    public ICommand CancelCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            ErrorOr<List<AccountDto>> accountsResult = await _billPaymentClientService.GetAccountsAsync();
            if (!accountsResult.IsError) Accounts = new ObservableCollection<AccountDto>(accountsResult.Value);

            ErrorOr<List<RecurringPaymentResponse>> paymentsResult =
                await _billPaymentClientService.GetRecurringPaymentsAsync();
            if (!paymentsResult.IsError)
                Payments = new ObservableCollection<RecurringPaymentResponse>(paymentsResult.Value);

            ErrorOr<List<BillerDto>> billersResult = await _billPaymentClientService.GetBillersAsync();
            if (!billersResult.IsError) Billers = new ObservableCollection<BillerDto>(billersResult.Value);
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    public Task CreateAsync() => ExecuteCreateAsync();

    public Task PauseAsync(RecurringPaymentResponse? payment) => ExecutePauseAsync(payment);

    public Task ResumeAsync(RecurringPaymentResponse? payment) => ExecuteResumeAsync(payment);

    public Task CancelAsync(RecurringPaymentResponse? payment) => ExecuteCancelAsync(payment);

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
        if (existingPayment == null) return;
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
        if (Equals(field, value)) return false;

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
                    await _executeAsyncNoParam();
                else
                    await _executeAsyncWithParam(parameter);
            }
            finally
            {
                _isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
