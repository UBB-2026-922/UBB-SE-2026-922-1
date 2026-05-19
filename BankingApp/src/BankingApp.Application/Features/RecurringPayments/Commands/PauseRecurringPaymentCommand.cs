namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Shared.Persistence;

public sealed record PauseRecurringPaymentCommand(int UserId, int PaymentId) : IRequest<ErrorOr<Success>>;

public sealed class PauseRecurringPaymentCommandHandler(
    IRecurringPaymentRepository recurringPaymentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PauseRecurringPaymentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(PauseRecurringPaymentCommand command, CancellationToken cancellationToken)
    {
        RecurringPayment? payment = await recurringPaymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null || payment.UserId != command.UserId)
        {
            return RecurringPaymentErrors.NotFound;
        }

        ErrorOr<Success> result = payment.Pause(command.UserId);
        if (result.IsError)
        {
            return result.FirstError;
        }

        await recurringPaymentRepository.UpdateAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
