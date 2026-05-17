namespace BankingApp.Application.Features.BillPayments.Queries;

using Domain.Aggregates.BillPaymentAggregate;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record GetBillPaymentHistoryQuery(int UserId)
    : IRequest<ErrorOr<List<BillPayResponse>>>;

public sealed class GetBillPaymentHistoryQueryHandler(IBillPaymentRepository billPaymentRepository)
    : IRequestHandler<GetBillPaymentHistoryQuery, ErrorOr<List<BillPayResponse>>>
{
    public async Task<ErrorOr<List<BillPayResponse>>> Handle(
        GetBillPaymentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<BillPayment> payments = await billPaymentRepository.ListByUserIdAsync(query.UserId, cancellationToken);

        return payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BillPayResponse
            {
                Id = p.Id,
                ReceiptNumber = p.ReceiptNumber,
                Fee = p.Fee.Amount,
                Amount = p.Amount.Amount,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            })
            .ToList();
    }
}
