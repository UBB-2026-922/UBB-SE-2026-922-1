namespace BankingApp.Application.Features.Transfers.Queries;

using Domain.Aggregates.TransferAggregate;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record GetTransferHistoryQuery(int UserId)
    : IRequest<ErrorOr<List<TransferResponse>>>;

public sealed class GetTransferHistoryQueryHandler(ITransferRepository transferRepository)
    : IRequestHandler<GetTransferHistoryQuery, ErrorOr<List<TransferResponse>>>
{
    public async Task<ErrorOr<List<TransferResponse>>> Handle(
        GetTransferHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Transfer> transfers = await transferRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return transfers
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransferResponse
            {
                Id = t.Id,
                SourceAccountId = t.SourceAccountId,
                TransactionId = t.LedgerTransactionId,
                RecipientName = t.RecipientName,
                RecipientIban = t.RecipientIban.Value,
                RecipientBankName = t.RecipientBankName,
                Amount = t.Amount.Amount,
                Currency = t.Amount.Currency.Code,
                Fee = t.Fee.Amount,
                Reference = t.Reference,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            })
            .ToList();
    }
}
