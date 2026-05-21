namespace BankingApp.Web.Controllers;

using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.RecurringPayments;

[Authorize]
public class RecurringPaymentsController(
    IRecurringPaymentService recurringPaymentService,
    IBillerService billerService,
    IBillPaymentService billPaymentService) : AuthenticatedController
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<List<RecurringPaymentResponse>> result =
            await recurringPaymentService.GetAllAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load recurring payments. Please try again.";
            return View(new RecurringPaymentListViewModel());
        }

        RecurringPaymentListViewModel viewModel = new()
        {
            Payments = result.Value.ConvertAll(payment => new RecurringPaymentRowViewModel
            {
                Id = payment.Id,
                BillerId = payment.BillerId,
                SourceAccountId = payment.SourceAccountId,
                Amount = payment.Amount,
                IsPayInFull = payment.IsPayInFull,
                Frequency = payment.Frequency,
                StartDate = payment.StartDate,
                EndDate = payment.EndDate,
                NextExecutionDate = payment.NextExecutionDate,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            })
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        CreateRecurringPaymentViewModel viewModel = new();
        await PopulateDropdownsAsync(viewModel, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRecurringPaymentViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        CreateRecurringPaymentRequest request = new()
        {
            BillerId = viewModel.SelectedBillerId,
            SourceAccountId = viewModel.SelectedSourceAccountId,
            Amount = viewModel.Amount,
            IsPayInFull = viewModel.IsPayInFull,
            Frequency = viewModel.Frequency,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate
        };

        ErrorOr<RecurringPaymentResponse> createResult =
            await recurringPaymentService.CreateAsync(request, cancellationToken);

        if (createResult.IsError)
        {
            ModelState.AddModelError(string.Empty, createResult.FirstError.Description);
            await PopulateDropdownsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        TempData["Success"] = "Recurring payment created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await recurringPaymentService.PauseAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? result.FirstError.Description
            : "Recurring payment paused.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await recurringPaymentService.ResumeAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? result.FirstError.Description
            : "Recurring payment resumed.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await recurringPaymentService.CancelAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? result.FirstError.Description
            : "Recurring payment cancelled.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(CreateRecurringPaymentViewModel viewModel, CancellationToken cancellationToken)
    {
        Task<ErrorOr<List<BillerDto>>> billersTask = billerService.GetBillersAsync(ct: cancellationToken);
        Task<ErrorOr<List<AccountDto>>> accountsTask = billPaymentService.GetAccountsAsync(cancellationToken);

        await Task.WhenAll(billersTask, accountsTask);

        ErrorOr<List<BillerDto>> billersResult = await billersTask;
        ErrorOr<List<AccountDto>> accountsResult = await accountsTask;

        viewModel.Billers = billersResult.IsError ? [] : billersResult.Value;
        viewModel.Accounts = accountsResult.IsError ? [] : accountsResult.Value;
    }
}
