namespace BankingApp.Web.Controllers;

using Contracts.Features.Transfers.Dtos;
using Contracts.Features.Transfers.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Transfers;

[Authorize]
public class TransfersController(ITransferService transferService) : AuthenticatedController
{
    public IActionResult New() => View(new TransferNewViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidateIban(TransferNewViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("New", model);
        }

        Task<ErrorOr<TransferIbanValidationResponse>> validationTask =
            transferService.ValidateIbanAsync(new TransferIbanValidationRequest { Iban = model.RecipientIban }, ct);
        Task<ErrorOr<List<TransferAccountSelectionResponse>>> accountsTask =
            transferService.GetAccountsAsync(ct);

        await Task.WhenAll(validationTask, accountsTask);

        ErrorOr<TransferIbanValidationResponse> validationResult = await validationTask;
        if (validationResult.IsError || !validationResult.Value.IsValid)
        {
            ModelState.AddModelError(string.Empty, "The IBAN is invalid. Please check and try again.");
            return View("New", model);
        }

        ErrorOr<List<TransferAccountSelectionResponse>> accountsResult = await accountsTask;
        if (accountsResult.IsError)
        {
            TempData["Error"] = "Unable to load your accounts. Please try again.";
            return View("New", model);
        }

        TransferIbanValidatedViewModel viewModel = new()
        {
            RecipientIban = model.RecipientIban,
            RecipientBankName = validationResult.Value.BankName,
            Accounts = accountsResult.Value,
        };

        return View("ValidateIban", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(TransferIbanValidatedViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateAccountsAsync(model, ct);
            return View("ValidateIban", model);
        }

        ErrorOr<List<TransferAccountSelectionResponse>> accountsResult = await transferService.GetAccountsAsync(ct);
        TransferAccountSelectionResponse? sourceAccount = accountsResult.IsError
            ? null
            : accountsResult.Value.FirstOrDefault(a => a.Id == model.SelectedAccountId);

        string sourceCurrency = sourceAccount?.Currency ?? string.Empty;

        TransferPreviewViewModel previewVm = new()
        {
            SourceAccountId = model.SelectedAccountId,
            SourceAccountIban = sourceAccount?.Iban ?? string.Empty,
            SourceCurrency = sourceCurrency,
            RecipientName = model.RecipientName,
            RecipientIban = model.RecipientIban,
            RecipientBankName = model.RecipientBankName,
            Amount = model.Amount,
            Currency = model.Currency,
            Reference = model.Reference,
        };

        // Check for cross-currency transfer
        bool isCrossCurrency = !string.IsNullOrEmpty(sourceCurrency)
                               && !string.Equals(sourceCurrency, model.Currency, StringComparison.OrdinalIgnoreCase);

        if (isCrossCurrency)
        {
            ErrorOr<TransferForexPreviewResponse> fxResult =
                await transferService.GetFxPreviewAsync(sourceCurrency, model.Currency, model.Amount, ct);

            if (fxResult.IsError)
            {
                ModelState.AddModelError(string.Empty, "Unable to fetch forex rate. Please try again.");
                await RepopulateAccountsAsync(model, ct);
                return View("ValidateIban", model);
            }

            previewVm.IsCrossCurrency = true;
            previewVm.ExchangeRate = fxResult.Value.ExchangeRate;
            previewVm.ConvertedAmount = fxResult.Value.ConvertedAmount;
        }

        return View("Preview", previewVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(TransferPreviewViewModel model, CancellationToken ct)
    {
        CreateTransferRequest request = new()
        {
            SourceAccountId = model.SourceAccountId,
            RecipientName = model.RecipientName,
            RecipientIban = model.RecipientIban,
            Amount = model.Amount,
            Currency = model.Currency,
            Reference = model.Reference,
        };

        ErrorOr<TransferExecutionResponse> result = await transferService.ExecuteAsync(request, ct);

        if (result.IsError)
        {
            TempData["Error"] = result.FirstError.Description;
            return RedirectToAction(nameof(New));
        }

        TempData["Success"] =
            $"Transfer completed! Ref: {result.Value.TransactionRef} — " +
            $"{model.Currency} {model.Amount:N2} to {model.RecipientName}.";

        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History(CancellationToken ct)
    {
        ErrorOr<List<TransferResponse>> result = await transferService.GetHistoryAsync(ct);

        if (result.IsError)
        {
            TempData["Error"] = "Unable to load transfer history. Please try again.";
            return View(new TransferHistoryViewModel());
        }

        TransferHistoryViewModel viewModel = new()
        {
            Transfers = result.Value.ConvertAll(t => new TransferHistoryRowViewModel
            {
                Id = t.Id,
                RecipientIban = t.RecipientIban,
                RecipientName = t.RecipientName,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
            })
        };

        return View(viewModel);
    }

    private async Task RepopulateAccountsAsync(TransferIbanValidatedViewModel vm, CancellationToken ct)
    {
        ErrorOr<List<TransferAccountSelectionResponse>> result = await transferService.GetAccountsAsync(ct);
        vm.Accounts = result.IsError ? [] : result.Value;
    }
}
