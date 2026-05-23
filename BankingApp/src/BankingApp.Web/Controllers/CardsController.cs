namespace BankingApp.Web.Controllers;

using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Contracts.Features.Cards.Services;
using BankingApp.Domain.Enums;
using BankingApp.Web.ViewModels.Cards;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Handles card listing, issuing, freezing, unfreezing and cancellation.</summary>
[Authorize]
public class CardsController(ICardService cardService) : Controller
{
    /// <summary>Displays the list of cards for the current user.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ErrorOr<List<CardDetailsDto>> result = await cardService.GetCardsAsync(cancellationToken);

        if (result.IsError)
        {
            TempData["Error"] = "Could not load cards. Please try again.";
            return View(new CardListViewModel());
        }

        CardListViewModel viewModel = new()
        {
            Cards = result.Value.ConvertAll(dto => new CardRowViewModel
            {
                Id = dto.Id,
                CardNumber = dto.CardNumber,
                FullCardNumber = dto.FullCardNumber,
                SecurityCode = dto.SecurityCode,
                CardholderName = dto.CardholderName,
                ExpiryDate = dto.ExpiryDate,
                CardType = dto.CardType,
                CardBrand = dto.CardBrand,
                Status = dto.Status,
                IsContactlessEnabled = dto.IsContactlessEnabled,
                IsOnlineEnabled = dto.IsOnlineEnabled,
                AccountName = dto.AccountName,
            })
        };

        return View(viewModel);
    }

    /// <summary>Displays the issue-card form.</summary>
    public IActionResult Issue() => View(new IssueCardViewModel());

    /// <summary>Processes the issue-card form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(IssueCardViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IssueCardRequest request = new()
        {
            CardType = model.CardType,
            CardBrand = model.CardBrand,
        };

        ErrorOr<CardDetailsDto> result = await cardService.IssueCardAsync(request, cancellationToken);

        if (result.IsError)
        {
            ModelState.AddModelError(string.Empty, "Could not issue card. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"Your new {model.CardBrand} {model.CardType} card has been issued successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Freezes the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Freeze(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.FreezeCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not freeze the card. Please try again."
            : "Card has been frozen successfully.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Unfreezes the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unfreeze(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.UnfreezeCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not unfreeze the card. Please try again."
            : "Card has been unfrozen successfully.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cancels the specified card.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await cardService.CancelCardAsync(id, cancellationToken);

        TempData[result.IsError ? "Error" : "Success"] = result.IsError
            ? "Could not cancel the card. Please try again."
            : "Card has been cancelled.";

        return RedirectToAction(nameof(Index));
    }
}
