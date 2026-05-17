namespace BankingApp.Application.Features.Cards.Queries;

using Common.Logging;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record GetCardsQuery(int UserId) : IRequest<ErrorOr<List<CardDetailsDto>>>;

public sealed class GetCardsQueryHandler(
    IAccountRepository accountRepository,
    ILogger<GetCardsQueryHandler> logger)
    : IRequestHandler<GetCardsQuery, ErrorOr<List<CardDetailsDto>>>
{
    public async Task<ErrorOr<List<CardDetailsDto>>> Handle(GetCardsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyCollection<Account> accounts = await accountRepository.ListByUserIdAsync(query.UserId, cancellationToken);

            var cards = new List<CardDetailsDto>();
            foreach (Account account in accounts)
            {
                foreach (Card card in account.Cards)
                {
                    cards.Add(new CardDetailsDto
                    {
                        Id = card.Id,
                        CardNumber = card.GetMaskedNumber(),
                        FullCardNumber = card.CardNumber,
                        SecurityCode = card.Cvv,
                        CardholderName = card.CardholderName,
                        ExpiryDate = card.ExpiryDate,
                        CardType = card.CardType,
                        CardBrand = card.CardBrand,
                        Status = card.Status,
                        IsContactlessEnabled = card.IsContactlessEnabled,
                        IsOnlineEnabled = card.IsOnlineEnabled,
                        AccountName = account.AccountName,
                        AccountId = account.Id
                    });
                }
            }

            return cards;
        }
        catch (Exception ex)
        {
            logger.GetCardsQueryFailed(query.UserId, ex.Message);
            return UserErrors.NotFound;
        }
    }
}
