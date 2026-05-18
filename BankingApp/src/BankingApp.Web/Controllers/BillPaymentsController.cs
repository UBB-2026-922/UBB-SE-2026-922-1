namespace BankingApp.Web.Controllers;

using System.Security.Claims;
using Application.Features.BillPayments.Commands;
using Application.Features.BillPayments.Queries;
using Application.Features.Billers.Dtos;
using Application.Features.Billers.Queries;
using Application.Features.BillPayments.Dtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.BillPayments;

[Authorize]
public class BillPaymentsController(ISender sender) : Controller
{
    private const decimal LowTierFee = 0.50m;
    private const decimal HighTierFee = 1.00m;
    private const decimal FeeThreshold = 100m;
    private const decimal TwoFaThreshold = 1_000m;

    /// <summary>
    ///     Renders the bill-payment entry form pre-populated with the user's
    ///     saved billers, all active billers (for search), and their accounts.
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        int userId = GetUserId();

        Task<ErrorOr<List<SavedBillerDto>>> savedBillersTask =
            sender.Send(new GetSavedBillersQuery(userId), cancellationToken);
        Task<ErrorOr<List<BillerDto>>> allBillersTask = sender.Send(new GetBillersQuery(), cancellationToken);
        Task<ErrorOr<List<AccountDto>>> accountsTask =
            sender.Send(new GetBillPayAccountsQuery(userId), cancellationToken);

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

    /// <summary>
    ///     Validates the form input, calculates the fee, checks whether 2FA is
    ///     required, and renders the confirmation/preview page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(BillPayViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateDropdownsAsync(viewModel, cancellationToken);
            return View("Index", viewModel);
        }

        ErrorOr<List<BillerDto>> allBillers = await sender.Send(new GetBillersQuery(), cancellationToken);
        string billerName = allBillers.IsError
            ? $"Biller #{viewModel.SelectedBillerId}"
            : allBillers.Value.FirstOrDefault(billerDto => billerDto.Id == viewModel.SelectedBillerId)?.Name
              ?? $"Biller #{viewModel.SelectedBillerId}";

        ErrorOr<List<AccountDto>> accounts =
            await sender.Send(new GetBillPayAccountsQuery(GetUserId()), cancellationToken);
        AccountDto? account =
            accounts.IsError ? null : accounts.Value.FirstOrDefault(a => a.Id == viewModel.SelectedAccountId);

        BillPayPreviewViewModel preview = new()
        {
            SourceAccountId = viewModel.SelectedAccountId,
            BillerId = viewModel.SelectedBillerId,
            BillerReference = viewModel.BillerReference,
            Amount = viewModel.Amount,
            Fee = CalculateFee(viewModel.Amount),
            BillerName = billerName,
            AccountIban = account?.Iban ?? string.Empty,
            Currency = account?.Currency ?? string.Empty,
            RequiresTwoFa = viewModel.Amount >= TwoFaThreshold,
        };

        return View("Preview", preview);
    }

    /// <summary>
    ///     Executes the bill payment.  When 2FA is required the OTP must be
    ///     present; if it is missing the preview page is re-displayed with an
    ///     inline validation error.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(BillPayPreviewViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.RequiresTwoFa && string.IsNullOrWhiteSpace(viewModel.TwoFaToken))
        {
            viewModel.ErrorMessage = "A one-time password is required for payments of $1,000 or more.";
            return View("Preview", viewModel);
        }

        ProcessBillPaymentCommand command = new(
            UserId: GetUserId(),
            SourceAccountId: viewModel.SourceAccountId,
            BillerId: viewModel.BillerId,
            BillerReference: viewModel.BillerReference,
            Amount: viewModel.Amount,
            TwoFaToken: viewModel.TwoFaToken);

        ErrorOr<BillPayResponse> result =
            await sender.Send(command, cancellationToken);

        if (result.IsError)
        {
            Error error = result.FirstError;

            if (error.Code == "BillPayment.TwoFactorRequired")
            {
                viewModel.RequiresTwoFa = true;
                viewModel.ErrorMessage = "This payment requires two-factor authentication. Enter your OTP below.";
                return View("Preview", viewModel);
            }

            TempData["Error"] = error.Description;
            return RedirectToAction(nameof(Index));
        }

        BillPayResponse response = result.Value;
        TempData["Success"] =
            $"Payment confirmed! Receipt: {response.ReceiptNumber} — " +
            $"Amount: {response.Amount:N2}, Fee: {response.Fee:N2}.";

        return RedirectToAction(nameof(History));
    }

    /// <summary>
    ///     Displays the authenticated user's bill-payment history, newest first.
    /// </summary>
    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        int userId = GetUserId();

        ErrorOr<List<BillPayResponse>> result =
            await sender.Send(new GetBillPaymentHistoryQuery(userId), cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load payment history. Please try again.";
            return View(new BillPaymentHistoryViewModel());
        }

        BillPaymentHistoryViewModel viewModel = new()
        {
            Payments = result.Value
                .ConvertAll(p => new BillPaymentRowViewModel
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

    /// <summary>
    ///     Extracts the authenticated user's ID from the cookie claims principal.
    ///     The claim is written as "userId" (matching the API's SessionValidationMiddleware)
    ///     or falls back to <see cref="ClaimTypes.NameIdentifier"/>.
    /// </summary>
    private int GetUserId()
    {
        string? raw = User.FindFirstValue("userId")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(raw, out int id) ? id : 0;
    }

    private static decimal CalculateFee(decimal amount) =>
        amount <= FeeThreshold ? LowTierFee : HighTierFee;

    /// <summary>Re-populates dropdowns on a <see cref="BillPayViewModel"/> after a failed validation.</summary>
    private async Task RepopulateDropdownsAsync(BillPayViewModel vm, CancellationToken ct)
    {
        int userId = GetUserId();

        Task<ErrorOr<List<SavedBillerDto>>> savedTask = sender.Send(new GetSavedBillersQuery(userId), ct);
        Task<ErrorOr<List<BillerDto>>> allTask = sender.Send(new GetBillersQuery(), ct);
        Task<ErrorOr<List<AccountDto>>> accountsTask = sender.Send(new GetBillPayAccountsQuery(userId), ct);
        await Task.WhenAll(savedTask, allTask, accountsTask);

        ErrorOr<List<SavedBillerDto>> savedResult = await savedTask;
        ErrorOr<List<BillerDto>> allResult = await allTask;
        ErrorOr<List<AccountDto>> accResult = await accountsTask;

        vm.SavedBillers = savedResult.IsError ? [] : savedResult.Value;
        vm.AllBillers = allResult.IsError ? [] : allResult.Value;
        vm.Accounts = accResult.IsError ? [] : accResult.Value;
    }
}