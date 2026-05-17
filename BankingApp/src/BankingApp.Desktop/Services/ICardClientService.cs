namespace BankingApp.Desktop.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.Features.Cards.Dtos;
using ErrorOr;

/// <summary>Defines the desktop client boundary for card management operations.</summary>
public interface ICardClientService
{
    /// <summary>Loads all cards for the authenticated user.</summary>
    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync();

    /// <summary>Issues a new card. The bank generates account and card details; only carrier and type are chosen.</summary>
    public Task<ErrorOr<CardDetailsDto>> IssueCardAsync(IssueCardRequest request);

    /// <summary>Freezes the card with the specified identifier.</summary>
    public Task<ErrorOr<Success>> FreezeCardAsync(int cardId);

    /// <summary>Unfreezes the card with the specified identifier.</summary>
    public Task<ErrorOr<Success>> UnfreezeCardAsync(int cardId);

    /// <summary>Cancels the card with the specified identifier.</summary>
    public Task<ErrorOr<Success>> CancelCardAsync(int cardId);
}
