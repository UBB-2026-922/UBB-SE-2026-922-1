namespace BankingApp.Web.Controllers;

using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Features.BillPayments.Dtos;
using Contracts.Features.BillPayments.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.BillPayments;

[Authorize]
public class BillPaymentsController(
    IBillPaymentService billPaymentService,
    IBillerService billerService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Task<ErrorOr<List<SavedBillerDto>>> savedBillersTask = billerService.GetSavedBillersAsync(cancellationToken);
        Task<ErrorOr<List<BillerDto>>> allBillersTask = billerService.GetBillersAsync(ct: cancellationToken);
        Task<ErrorOr<List<AccountDto>>> accountsTask = billPaymentService.GetAccountsAsync(cancellationToken);

        await Task.WhenAll(savedBillersTask, allBillersTask, accountsTask);

        ErrorOr<List<BillerDto>> allBillersResult = await allBillersTask;
        if (allBillersResult.IsError)
        {
            TempData["Error"] = "Unable to load billers. Please try again.";
            return View(new BillPayViewModel());
        }

        ErrorOr<List<AccountDto>> accountsResult = await accountsTask;
        if (accountsResult.IsError)
        {
            TempData["Error"] = "Unable to load your accounts. Please try again.";
            return View(new BillPayViewModel());
        }

        ErrorOr<List<SavedBillerDto>> savedResult = await savedBillersTask;

        BillPayViewModel viewModel = new()
        {
            SavedBillers = savedResult.IsError ? [] : savedResult.Value,
            AllBillers = allBillersResult.Value,
            Accounts = accountsResult.Value,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(BillPayViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateDropdownsAsync(viewModel, cancellationToken);
            return View("Index", viewModel);
        }

        Task<ErrorOr<List<BillerDto>>> allBillersTask = billerService.GetBillersAsync(ct: cancellationToken);
        Task<ErrorOr<List<AccountDto>>> accountsTask = billPaymentService.GetAccountsAsync(cancellationToken);
        Task<ErrorOr<FeeResponse>> feeTask = billPaymentService.GetFeeAsync(viewModel.Amount, cancellationToken);

        await Task.WhenAll(allBillersTask, accountsTask, feeTask);

        ErrorOr<List<BillerDto>> allBillers = await allBillersTask;
        string billerName = allBillers.IsError
            ? $"Biller #{viewModel.SelectedBillerId}"
            : allBillers.Value.FirstOrDefault(b => b.Id == viewModel.SelectedBillerId)?.Name
              ?? $"Biller #{viewModel.SelectedBillerId}";

        ErrorOr<List<AccountDto>> accounts = await accountsTask;
        AccountDto? account = accounts.IsError ? null : accounts.Value.FirstOrDefault(account => account.Id == viewModel.SelectedAccountId);

        ErrorOr<FeeResponse> feeResult = await feeTask;

        BillPayPreviewViewModel preview = new()
        {
            SourceAccountId = viewModel.SelectedAccountId,
            BillerId = viewModel.SelectedBillerId,
            BillerReference = viewModel.BillerReference,
            Amount = viewModel.Amount,
            Fee = feeResult.IsError ? FallbackFee(viewModel.Amount) : feeResult.Value.Fee,
            BillerName = billerName,
            AccountIban = account?.Iban ?? string.Empty,
            Currency = account?.Currency ?? string.Empty,
        };

        return View("Preview", preview);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(BillPayPreviewViewModel viewModel, CancellationToken cancellationToken)
    {
        BillPayRequest request = new()
        {
            SourceAccountId = viewModel.SourceAccountId,
            BillerId = viewModel.BillerId,
            BillerReference = viewModel.BillerReference,
            Amount = viewModel.Amount,
        };

        ErrorOr<BillPayResponse> result = await billPaymentService.PayBillAsync(request, cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = result.FirstError.Description;
            return RedirectToAction(nameof(Index));
        }

        BillPayResponse response = result.Value;
        TempData["Success"] =
            $"Payment confirmed! Receipt: {response.ReceiptNumber} — " +
            $"Amount: {response.Amount:N2}, Fee: {response.Fee:N2}.";

        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        ErrorOr<List<BillPayResponse>> result = await billPaymentService.GetHistoryAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load payment history. Please try again.";
            return View(new BillPaymentHistoryViewModel());
        }

        BillPaymentHistoryViewModel viewModel = new()
        {
            Payments = result.Value.ConvertAll(p => new BillPaymentRowViewModel
            {
                Id = p.Id,
                ReceiptNumber = p.ReceiptNumber,
                Amount = p.Amount,
                Fee = p.Fee,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
        };

        return View(viewModel);
    }

    private static decimal FallbackFee(decimal amount) => amount <= 100m ? 0.50m : 1.00m;

    private async Task RepopulateDropdownsAsync(BillPayViewModel viewModel, CancellationToken cancellationToken)
    {
        Task<ErrorOr<List<SavedBillerDto>>> savedTask = billerService.GetSavedBillersAsync(cancellationToken);
        Task<ErrorOr<List<BillerDto>>> allTask = billerService.GetBillersAsync(ct: cancellationToken);
        Task<ErrorOr<List<AccountDto>>> accountsTask = billPaymentService.GetAccountsAsync(cancellationToken);
        await Task.WhenAll(savedTask, allTask, accountsTask);

        ErrorOr<List<SavedBillerDto>> savedResult = await savedTask;
        ErrorOr<List<BillerDto>> allResult = await allTask;
        ErrorOr<List<AccountDto>> accResult = await accountsTask;

        viewModel.SavedBillers = savedResult.IsError ? [] : savedResult.Value;
        viewModel.AllBillers = allResult.IsError ? [] : allResult.Value;
        viewModel.Accounts = accResult.IsError ? [] : accResult.Value;
    }
}
