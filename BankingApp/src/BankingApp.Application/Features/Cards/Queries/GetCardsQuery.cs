namespace BankingApp.Application.Features.Cards.Queries;

using Common.Logging;
using Contracts.Features.Cards.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

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

            return (from account in accounts
                from card in account.Cards
                select new CardDetailsDto
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
                }).ToList();
        }
        catch (Exception excpetion)
        {
            ApplicationLogMessages.GetCardsQueryFailed(logger, query.UserId, excpetion.Message);
            return UserErrors.NotFound;
        }
    }
}
