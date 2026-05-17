namespace BankingApp.Web.Controllers;

using System.Security.Claims;
using BankingApp.Application.Features.BillPayments.Commands;
using BankingApp.Application.Features.BillPayments.Queries;
using BankingApp.Application.Features.Billers.Dtos;
using BankingApp.Application.Features.Billers.Queries;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.BillPayments;

/// <summary>
///     Handles bill-payment operations:
///     pay form, fee preview, payment confirmation, and history.
/// </summary>
[Authorize]
public class BillPaymentsController(ISender sender) : Controller
{
    // ── Domain constants (mirrors BillPaymentFeePolicy + API Requires2Fa) ────
    private const decimal LowTierFee       = 0.50m;
    private const decimal HighTierFee      = 1.00m;
    private const decimal FeeThreshold     = 100m;
    private const decimal TwoFaThreshold   = 1_000m;

    // =========================================================================
    // GET /BillPayments
    // =========================================================================

    /// <summary>
    ///     Renders the bill-payment entry form pre-populated with the user's
    ///     saved billers, all active billers (for search), and their accounts.
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        int userId = GetUserId();

        Task<ErrorOr<List<Application.Features.Billers.Dtos.SavedBillerDto>>> savedBillersTask  = sender.Send(new GetSavedBillersQuery(userId), cancellationToken);
        Task<ErrorOr<List<BillerDto>>>                                          allBillersTask    = sender.Send(new GetBillersQuery(), cancellationToken);
        Task<ErrorOr<List<Application.Features.BillPayments.Dtos.AccountDto>>> accountsTask      = sender.Send(new GetBillPayAccountsQuery(userId), cancellationToken);

        await Task.WhenAll(savedBillersTask, allBillersTask, accountsTask);

        ErrorOr<List<BillerDto>> allBillersResult = await allBillersTask;
        if (allBillersResult.IsError)
        {
            TempData["Error"] = "Unable to load billers. Please try again.";
            return View(new BillPayViewModel());
        }

        ErrorOr<List<Application.Features.BillPayments.Dtos.AccountDto>> accountsResult = await accountsTask;
        if (accountsResult.IsError)
        {
            TempData["Error"] = "Unable to load your accounts. Please try again.";
            return View(new BillPayViewModel());
        }

        ErrorOr<List<Application.Features.Billers.Dtos.SavedBillerDto>> savedResult = await savedBillersTask;

        var vm = new BillPayViewModel
        {
            SavedBillers = savedResult.IsError ? [] : savedResult.Value,
            AllBillers   = allBillersResult.Value,
            Accounts     = accountsResult.Value,
        };

        return View(vm);
    }

    // =========================================================================
    // POST /BillPayments/Preview
    // =========================================================================

    /// <summary>
    ///     Validates the form input, calculates the fee, checks whether 2FA is
    ///     required, and renders the confirmation/preview page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(BillPayViewModel vm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Re-populate dropdowns before returning the form.
            await RepopulateDropdownsAsync(vm, cancellationToken);
            return View("Index", vm);
        }

        // Look up biller display name.
        ErrorOr<List<BillerDto>> allBillers = await sender.Send(new GetBillersQuery(), cancellationToken);
        string billerName = allBillers.IsError
            ? $"Biller #{vm.SelectedBillerId}"
            : allBillers.Value.FirstOrDefault(b => b.Id == vm.SelectedBillerId)?.Name
              ?? $"Biller #{vm.SelectedBillerId}";

        // Look up account display info.
        ErrorOr<List<Application.Features.BillPayments.Dtos.AccountDto>> accounts =
            await sender.Send(new GetBillPayAccountsQuery(GetUserId()), cancellationToken);
        Application.Features.BillPayments.Dtos.AccountDto? account =
            accounts.IsError ? null : accounts.Value.FirstOrDefault(a => a.Id == vm.SelectedAccountId);

        decimal fee = CalculateFee(vm.Amount);
        bool requiresTwoFa = vm.Amount >= TwoFaThreshold;

        var preview = new BillPayPreviewViewModel
        {
            SourceAccountId = vm.SelectedAccountId,
            BillerId        = vm.SelectedBillerId,
            BillerReference = vm.BillerReference,
            Amount          = vm.Amount,
            Fee             = fee,
            BillerName      = billerName,
            AccountIban     = account?.Iban ?? string.Empty,
            Currency        = account?.Currency ?? string.Empty,
            RequiresTwoFa   = requiresTwoFa,
        };

        return View("Preview", preview);
    }

    // =========================================================================
    // POST /BillPayments/Confirm
    // =========================================================================

    /// <summary>
    ///     Executes the bill payment.  When 2FA is required the OTP must be
    ///     present; if it is missing the preview page is re-displayed with an
    ///     inline validation error.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(BillPayPreviewViewModel vm, CancellationToken cancellationToken)
    {
        // Guard: 2FA token required but not supplied → re-show preview.
        if (vm.RequiresTwoFa && string.IsNullOrWhiteSpace(vm.TwoFaToken))
        {
            vm.ErrorMessage = "A one-time password is required for payments of $1,000 or more.";
            return View("Preview", vm);
        }

        int userId = GetUserId();

        var command = new ProcessBillPaymentCommand(
            UserId:          userId,
            SourceAccountId: vm.SourceAccountId,
            BillerId:        vm.BillerId,
            BillerReference: vm.BillerReference,
            Amount:          vm.Amount,
            TwoFaToken:      vm.TwoFaToken);

        ErrorOr<Application.Features.BillPayments.Dtos.BillPayResponse> result =
            await sender.Send(command, cancellationToken);

        if (result.IsError)
        {
            ErrorOr.Error error = result.FirstError;

            // 2FA specifically required by the domain → re-show preview with
            // the 2FA field visible and a contextual error.
            if (error.Code == "BillPayment.TwoFactorRequired")
            {
                vm.RequiresTwoFa = true;
                vm.ErrorMessage  = "This payment requires two-factor authentication. Enter your OTP below.";
                return View("Preview", vm);
            }

            TempData["Error"] = error.Description;
            return RedirectToAction(nameof(Index));
        }

        Application.Features.BillPayments.Dtos.BillPayResponse response = result.Value;
        TempData["Success"] =
            $"Payment confirmed! Receipt: {response.ReceiptNumber} — " +
            $"Amount: {response.Amount:N2}, Fee: {response.Fee:N2}.";

        return RedirectToAction(nameof(History));
    }

    // =========================================================================
    // GET /BillPayments/History
    // =========================================================================

    /// <summary>
    ///     Displays the authenticated user's bill-payment history, newest first.
    /// </summary>
    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        int userId = GetUserId();

        ErrorOr<List<Application.Features.BillPayments.Dtos.BillPayResponse>> result =
            await sender.Send(new GetBillPaymentHistoryQuery(userId), cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load payment history. Please try again.";
            return View(new BillPaymentHistoryViewModel());
        }

        var vm = new BillPaymentHistoryViewModel
        {
            Payments = result.Value
                .Select(p => new BillPaymentRowViewModel
                {
                    Id            = p.Id,
                    ReceiptNumber = p.ReceiptNumber,
                    Amount        = p.Amount,
                    Fee           = p.Fee,
                    Status        = p.Status,
                    CreatedAt     = p.CreatedAt
                })
                .ToList()
        };

        return View(vm);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

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

        Task<ErrorOr<List<Application.Features.Billers.Dtos.SavedBillerDto>>> savedTask    = sender.Send(new GetSavedBillersQuery(userId), ct);
        Task<ErrorOr<List<BillerDto>>>                                          allTask      = sender.Send(new GetBillersQuery(), ct);
        Task<ErrorOr<List<Application.Features.BillPayments.Dtos.AccountDto>>> accountsTask = sender.Send(new GetBillPayAccountsQuery(userId), ct);
        await Task.WhenAll(savedTask, allTask, accountsTask);

        ErrorOr<List<Application.Features.Billers.Dtos.SavedBillerDto>> savedResult = await savedTask;
        ErrorOr<List<BillerDto>> allResult = await allTask;
        ErrorOr<List<Application.Features.BillPayments.Dtos.AccountDto>> accResult = await accountsTask;

        vm.SavedBillers = savedResult.IsError ? [] : savedResult.Value;
        vm.AllBillers   = allResult.IsError ? [] : allResult.Value;
        vm.Accounts     = accResult.IsError ? [] : accResult.Value;
    }
}
