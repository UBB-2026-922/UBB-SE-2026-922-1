namespace BankingApp.Application.Features.Forex.Queries;

using Domain.Aggregates.ForexAggregate;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record GetForexHistoryQuery(int UserId) : IRequest<ErrorOr<List<ForexTransactionResponse>>>;

public sealed class GetForexHistoryQueryHandler(IForexRepository forexRepository)
    : IRequestHandler<GetForexHistoryQuery, ErrorOr<List<ForexTransactionResponse>>>
{
    public async Task<ErrorOr<List<ForexTransactionResponse>>> Handle(GetForexHistoryQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ForexTransaction> transactions = await forexRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return transactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ForexTransactionResponse
            {
                Id = t.Id,
                SourceCurrency = t.SourceAmount.Currency.Code,
                TargetCurrency = t.TargetAmount.Currency.Code,
                TargetAmount = t.TargetAmount.Amount,
                ExchangeRate = t.ExchangeRate,
                Commission = t.Commission.Amount,
                Status = t.Status
            })
            .ToList();
    }
}
