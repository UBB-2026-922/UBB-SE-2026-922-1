namespace BankingApp.Application.Features.RecurringPayments.Queries;

using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;

public sealed record GetRecurringPaymentsQuery(int UserId) : IRequest<ErrorOr<List<RecurringPaymentResponse>>>;

public sealed class GetRecurringPaymentsQueryHandler(IRecurringPaymentRepository recurringPaymentRepository)
    : IRequestHandler<GetRecurringPaymentsQuery, ErrorOr<List<RecurringPaymentResponse>>>
{
    public async Task<ErrorOr<List<RecurringPaymentResponse>>> Handle(GetRecurringPaymentsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RecurringPayment> payments = await recurringPaymentRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        return payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new RecurringPaymentResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                BillerId = p.BillerId,
                SourceAccountId = p.SourceAccountId,
                Amount = p.Amount,
                IsPayInFull = p.IsPayInFull,
                Frequency = p.Frequency,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                NextExecutionDate = p.NextExecutionDate,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToList();
    }
}
