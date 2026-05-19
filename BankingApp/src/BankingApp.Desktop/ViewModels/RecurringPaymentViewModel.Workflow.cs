namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Domain.Enums;
using Contracts.Features.Billers.Dtos;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Features.RecurringPayments.Services;
using ErrorOr;

public partial class RecurringPaymentViewModel
{
    /// <summary>Loads the accounts, billers, and existing recurring payments for the current user.</summary>
    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            await LoadAccountsAsync();
            await LoadPaymentsAsync();
            await LoadBillersAsync();
        }
        catch (Exception loadException)
        {
            ErrorMessage = $"Failed to load data: {loadException.Message}";
        }
    }

    private async Task ExecuteCreateAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!ValidateCreateRequest())
            {
                return;
            }

            CreateRecurringPaymentRequest request = BuildCreateRequest();
            ErrorOr<RecurringPaymentResponse> result =
                await _recurringPaymentService.CreateAsync(request);

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

    private async Task ExecutePauseAsync(RecurringPaymentResponse? payment)
    {
        await ChangePaymentStatusAsync(
            payment,
            "Please select a recurring payment to pause.",
            (id, ct) => _recurringPaymentService.PauseAsync(id, ct),
            RecurringPaymentStatus.Paused,
            "pause");
    }

    private async Task ExecuteResumeAsync(RecurringPaymentResponse? payment)
    {
        await ChangePaymentStatusAsync(
            payment,
            "Please select a recurring payment to resume.",
            (id, ct) => _recurringPaymentService.ResumeAsync(id, ct),
            RecurringPaymentStatus.Active,
            "resume");
    }

    private async Task ExecuteCancelAsync(RecurringPaymentResponse? payment)
    {
        await ChangePaymentStatusAsync(
            payment,
            "Please select a recurring payment to cancel.",
            (id, ct) => _recurringPaymentService.CancelAsync(id, ct),
            RecurringPaymentStatus.Cancelled,
            "cancel");
    }

    private async Task LoadAccountsAsync()
    {
        ErrorOr<List<AccountDto>> result = await _billPaymentService.GetAccountsAsync();
        if (!result.IsError)
        {
            Accounts = new ObservableCollection<AccountDto>(result.Value);
        }
    }

    private async Task LoadPaymentsAsync()
    {
        ErrorOr<List<RecurringPaymentResponse>> result = await _recurringPaymentService.GetAllAsync();
        if (!result.IsError)
        {
            Payments = new ObservableCollection<RecurringPaymentResponse>(result.Value);
        }
    }

    private async Task LoadBillersAsync()
    {
        ErrorOr<List<BillerDto>> result = await _billerService.GetBillersAsync();
        if (!result.IsError)
        {
            Billers = new ObservableCollection<BillerDto>(result.Value);
        }
    }

    private bool ValidateCreateRequest()
    {
        if (SelectedBiller == null)
        {
            ErrorMessage = "Please select a biller.";
            return false;
        }

        if (SelectedAccount == null)
        {
            ErrorMessage = "Please select a source account.";
            return false;
        }

        if (Amount <= NoAmount)
        {
            ErrorMessage = "Please enter a valid amount.";
            return false;
        }

        if (EndDate.HasValue && EndDate.Value.Date < StartDate.Date)
        {
            ErrorMessage = "End date cannot be earlier than start date.";
            return false;
        }

        return true;
    }

    private CreateRecurringPaymentRequest BuildCreateRequest() =>
        new()
        {
            BillerId = SelectedBiller!.Id,
            SourceAccountId = SelectedAccount!.Id,
            Amount = Amount,
            IsPayInFull = false,
            Frequency = Frequency,
            StartDate = StartDate,
            EndDate = EndDate,
        };

    private async Task ChangePaymentStatusAsync(
            RecurringPaymentResponse? payment,
            string missingSelectionMessage,
            Func<int, CancellationToken, Task<ErrorOr<Success>>> operation,
            RecurringPaymentStatus newStatus,
            string operationName)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (payment == null)
            {
                ErrorMessage = missingSelectionMessage;
                return;
            }

            ErrorOr<Success> result = await operation(payment.Id, CancellationToken.None);
            if (result.IsError)
            {
                ErrorMessage = result.FirstError.Description;
                return;
            }

            UpdatePaymentInCollection(payment.Id, newStatus);
        }
        catch (Exception operationException)
        {
            ErrorMessage = $"Failed to {operationName} recurring payment: {operationException.Message}";
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
}
