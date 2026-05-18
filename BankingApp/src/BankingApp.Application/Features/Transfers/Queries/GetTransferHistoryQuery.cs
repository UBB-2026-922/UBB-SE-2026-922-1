namespace BankingApp.Application.Features.Transfers.Queries;

using Contracts.Features.Transfers.Dtos;
using Domain.Aggregates.TransferAggregate;
using Domain.Repositories;
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
        IReadOnlyCollection<Transfer> transfers =
            await transferRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return transfers
            .OrderByDescending(transfer => transfer.CreatedAt)
            .Select(transfer => new TransferResponse
            {
                Id = transfer.Id,
                SourceAccountId = transfer.SourceAccountId,
                TransactionId = transfer.LedgerTransactionId,
                RecipientName = transfer.RecipientName,
                RecipientIban = transfer.RecipientIban.Value,
                RecipientBankName = transfer.RecipientBankName,
                Amount = transfer.Amount.Amount,
                Currency = transfer.Amount.Currency.Code,
                Fee = transfer.Fee.Amount,
                Reference = transfer.Reference,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt
            })
            .ToList();
    }
}