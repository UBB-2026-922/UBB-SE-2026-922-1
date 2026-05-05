// <copyright file="RecurringPaymentViewModel.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Drives the recurring payment management screen.
///     Migrated from BankingAppTeamB and adapted to use <see cref="IApiClient" />
///     for all server communication instead of the Team B local service layer.
/// </summary>
public class RecurringPaymentViewModel : INotifyPropertyChanged
{
    private const int NoBillerSelected = 0;
    private const decimal NoAmount = 0m;

    private readonly IApiClient _apiClient;

    private ObservableCollection<RecurringPaymentDto> _payments;
    private RecurringPaymentDto? _selectedPayment;
    private int _selectedBillerId;
    private decimal _amount;
    private RecurringFrequency _frequency;
    private DateTime _startDate;
    private DateTime? _endDate;
    private string _errorMessage = string.Empty;

    private ObservableCollection<TransferAccountDto> _accounts;
    private TransferAccountDto? _selectedAccount;

    private ObservableCollection<BillerDto> _billers;
    private BillerDto? _selectedBiller;

    private ObservableCollection<RecurringFrequency> _frequencies;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for all recurring payment-related server calls.</param>
    public RecurringPaymentViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        _payments = new ObservableCollection<RecurringPaymentDto>();
        _accounts = new ObservableCollection<TransferAccountDto>();
        _billers = new ObservableCollection<BillerDto>();
        _frequencies = new ObservableCollection<RecurringFrequency>
        {
            RecurringFrequency.Weekly,
            RecurringFrequency.Monthly,
            RecurringFrequency.Quarterly,
        };

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
    ///     Gets or sets the observable collection of recurring payments.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableCollection<RecurringPaymentDto> Payments
    {
        get => _payments;
        set => SetProperty(ref _payments, value);
    }

    /// <summary>
    ///     Gets or sets the currently selected recurring payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public RecurringPaymentDto? SelectedPayment
    {
        get => _selectedPayment;
        set => SetProperty(ref _selectedPayment, value);
    }

    /// <summary>
    ///     Gets or sets the identifier of the selected biller.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SelectedBillerId
    {
        get => _selectedBillerId;
        set => SetProperty(ref _selectedBillerId, value);
    }

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    /// <summary>
    ///     Gets or sets the payment frequency.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public RecurringFrequency Frequency
    {
        get => _frequency;
        set => SetProperty(ref _frequency, value);
    }

    /// <summary>
    ///     Gets or sets the start date for the recurring payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    /// <summary>
    ///     Gets or sets the optional end date for the recurring payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    /// <summary>
    ///     Gets or sets an error message indicating a problem with the current operation.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether there is a current error message.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    ///     Gets or sets the observable collection of transfer accounts available as a source.
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
    ///     Gets or sets the selected source account for the payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferAccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    /// <summary>
    ///     Gets or sets the observable collection of billers available.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableCollection<BillerDto> Billers
    {
        get => _billers;
        set => SetProperty(ref _billers, value);
    }

    /// <summary>
    ///     Gets or sets the selected biller for the payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
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
    ///     Gets or sets the available frequencies for a recurring payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableCollection<RecurringFrequency> Frequencies
    {
        get => _frequencies;
        set => SetProperty(ref _frequencies, value);
    }

    /// <summary>Gets the command that initiates creation of a new recurring payment.</summary>
    public ICommand CreateCommand { get; }

    /// <summary>Gets the command to pause the selected recurring payment.</summary>
    public ICommand PauseCommand { get; }

    /// <summary>Gets the command to resume the selected recurring payment.</summary>
    public ICommand ResumeCommand { get; }

    /// <summary>Gets the command to cancel the selected recurring payment.</summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Loads the user's recurring payments, accounts, and the biller directory from the API.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            // Load Accounts
            ErrorOr<List<TransferAccountDto>> accountsResult = await _apiClient.GetAsync<List<TransferAccountDto>>(ApiEndpoints.TransferAccounts);
            if (!accountsResult.IsError)
            {
                Accounts = new ObservableCollection<TransferAccountDto>(accountsResult.Value);
            }

            // Load Payments
            ErrorOr<List<RecurringPaymentDto>> paymentsResult = await _apiClient.GetAsync<List<RecurringPaymentDto>>(ApiEndpoints.RecurringPayments);
            if (!paymentsResult.IsError)
            {
                Payments = new ObservableCollection<RecurringPaymentDto>(paymentsResult.Value);
            }

            // Load Billers
            ErrorOr<List<BillerDto>> billersResult = await _apiClient.GetAsync<List<BillerDto>>(ApiEndpoints.Billers);
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

    /// <summary>Public facade that delegates to ExecuteCreateAsync.</summary>
    public Task CreateAsync() => ExecuteCreateAsync();

    /// <summary>Public facade that delegates to ExecutePauseAsync.</summary>
    public Task PauseAsync(RecurringPaymentDto? payment) => ExecutePauseAsync(payment);

    /// <summary>Public facade that delegates to ExecuteResumeAsync.</summary>
    public Task ResumeAsync(RecurringPaymentDto? payment) => ExecuteResumeAsync(payment);

    /// <summary>Public facade that delegates to ExecuteCancelAsync.</summary>
    public Task CancelAsync(RecurringPaymentDto? payment) => ExecuteCancelAsync(payment);

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event for the specified property.
    /// </summary>
    internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Validates form inputs, creates the recurring payment via the API, and adds the result to the Payments collection.
    /// </summary>
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

            var request = new RecurringPaymentDto
            {
                BillerId = SelectedBiller.Id,
                SourceAccountId = SelectedAccount.Id,
                Amount = Amount,
                IsPayInFull = false,
                Frequency = Frequency,
                StartDate = StartDate,
                EndDate = EndDate,
            };

            ErrorOr<RecurringPaymentDto> result = await _apiClient.PostAsync<RecurringPaymentDto, RecurringPaymentDto>(
                ApiEndpoints.RecurringPayments,
                request);

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

    /// <summary>
    ///     Pauses the recurring payment supplied as parameter via the API.
    /// </summary>
    private async Task ExecutePauseAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentDto payment)
            {
                ErrorMessage = "Please select a recurring payment to pause.";
                return;
            }

            ErrorOr<RecurringPaymentDto> result = await _apiClient.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/pause",
                new { });

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

    /// <summary>
    ///     Resumes the recurring payment supplied as parameter via the API.
    /// </summary>
    private async Task ExecuteResumeAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentDto payment)
            {
                ErrorMessage = "Please select a recurring payment to resume.";
                return;
            }

            ErrorOr<RecurringPaymentDto> result = await _apiClient.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/resume",
                new { });

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

    /// <summary>
    ///     Cancels the recurring payment supplied as parameter via the API.
    /// </summary>
    private async Task ExecuteCancelAsync(object? parameter)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (parameter is not RecurringPaymentDto payment)
            {
                ErrorMessage = "Please select a recurring payment to cancel.";
                return;
            }

            ErrorOr<RecurringPaymentDto> result = await _apiClient.PutAsync<object, RecurringPaymentDto>(
                $"{ApiEndpoints.RecurringPayments}/{payment.Id}/cancel",
                new { });

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
        var existingPayment = Payments.FirstOrDefault(p => p.Id == paymentId);
        if (existingPayment != null)
        {
            var index = Payments.IndexOf(existingPayment);
            existingPayment.Status = newStatus;
            Payments[index] = existingPayment;
        }
    }

    /// <summary>Resets all create-form fields to their initial values.</summary>
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

    /// <summary>
    ///     Sets a field value and raises <see cref="PropertyChanged" /> when the value changes.
    /// </summary>
    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) { return false; }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    ///     Asynchronous relay command that prevents re-entrant execution.
    /// </summary>
    private sealed class AsyncRelayCommand : ICommand
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