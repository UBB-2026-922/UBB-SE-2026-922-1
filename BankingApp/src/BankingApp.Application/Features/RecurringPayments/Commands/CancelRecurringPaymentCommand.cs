namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Common.Contracts;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record CancelRecurringPaymentCommand(int UserId, int PaymentId) : IRequest<ErrorOr<Success>>;

public sealed class CancelRecurringPaymentCommandHandler(
    IRecurringPaymentRepository recurringPaymentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelRecurringPaymentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(CancelRecurringPaymentCommand command, CancellationToken cancellationToken)
    {
        RecurringPayment? payment = await recurringPaymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null || payment.UserId != command.UserId)
        {
            return RecurringPaymentErrors.NotFound;
        }

        ErrorOr<Success> result = payment.Cancel(command.UserId);
        if (result.IsError)
        {
            return result.FirstError;
        }

        await recurringPaymentRepository.UpdateAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
