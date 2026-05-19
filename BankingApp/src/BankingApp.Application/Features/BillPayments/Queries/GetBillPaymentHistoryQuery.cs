namespace BankingApp.Application.Features.BillPayments.Queries;

using Contracts.Features.BillPayments.Dtos;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Repositories;
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
            .OrderByDescending(billPayment => billPayment.CreatedAt)
            .Select(billPayment => new BillPayResponse
            {
                Id = billPayment.Id,
                ReceiptNumber = billPayment.ReceiptNumber,
                Fee = billPayment.Fee.Amount,
                Amount = billPayment.Amount.Amount,
                Status = billPayment.Status.ToString(),
                CreatedAt = billPayment.CreatedAt
            })
            .ToList();
    }
}
