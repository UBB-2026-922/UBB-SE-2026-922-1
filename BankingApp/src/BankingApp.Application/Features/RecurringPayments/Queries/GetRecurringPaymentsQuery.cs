namespace BankingApp.Application.Features.RecurringPayments.Queries;

using Contracts.Features.RecurringPayments.Dtos;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetRecurringPaymentsQuery(int UserId) : IRequest<ErrorOr<List<RecurringPaymentResponse>>>;

public sealed class GetRecurringPaymentsQueryHandler(IRecurringPaymentRepository recurringPaymentRepository)
    : IRequestHandler<GetRecurringPaymentsQuery, ErrorOr<List<RecurringPaymentResponse>>>
{
    public async Task<ErrorOr<List<RecurringPaymentResponse>>> Handle(GetRecurringPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RecurringPayment> payments =
            await recurringPaymentRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        return payments
            .OrderByDescending(recurringPayment => recurringPayment.CreatedAt)
            .Select(recurringPayment => new RecurringPaymentResponse
            {
                Id = recurringPayment.Id,
                UserId = recurringPayment.UserId,
                BillerId = recurringPayment.BillerId,
                SourceAccountId = recurringPayment.SourceAccountId,
                Amount = recurringPayment.Amount,
                IsPayInFull = recurringPayment.IsPayInFull,
                Frequency = recurringPayment.Frequency,
                StartDate = recurringPayment.StartDate,
                EndDate = recurringPayment.EndDate,
                NextExecutionDate = recurringPayment.NextExecutionDate,
                Status = recurringPayment.Status,
                CreatedAt = recurringPayment.CreatedAt
            })
            .ToList();
    }
}