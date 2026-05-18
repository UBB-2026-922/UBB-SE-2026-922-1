namespace BankingApp.Application.Features.Forex.Queries;

using Contracts.Features.Forex.Dtos;
using Domain.Aggregates.ForexAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetForexHistoryQuery(int UserId) : IRequest<ErrorOr<List<ForexTransactionResponse>>>;

public sealed class GetForexHistoryQueryHandler(IForexRepository forexRepository)
    : IRequestHandler<GetForexHistoryQuery, ErrorOr<List<ForexTransactionResponse>>>
{
    public async Task<ErrorOr<List<ForexTransactionResponse>>> Handle(GetForexHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ForexTransaction> transactions =
            await forexRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return transactions
            .OrderByDescending(forexTransaction => forexTransaction.CreatedAt)
            .Select(forexTransaction => new ForexTransactionResponse
            {
                Id = forexTransaction.Id,
                SourceCurrency = forexTransaction.SourceAmount.Currency.Code,
                TargetCurrency = forexTransaction.TargetAmount.Currency.Code,
                TargetAmount = forexTransaction.TargetAmount.Amount,
                ExchangeRate = forexTransaction.ExchangeRate,
                Commission = forexTransaction.Commission.Amount,
                Status = forexTransaction.Status
            })
            .ToList();
    }
}