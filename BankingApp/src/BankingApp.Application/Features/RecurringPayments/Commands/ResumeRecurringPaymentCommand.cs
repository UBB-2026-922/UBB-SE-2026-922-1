namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Common.Contracts;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record ResumeRecurringPaymentCommand(int UserId, int PaymentId) : IRequest<ErrorOr<Success>>;

public sealed class ResumeRecurringPaymentCommandHandler(
    IRecurringPaymentRepository recurringPaymentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResumeRecurringPaymentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ResumeRecurringPaymentCommand command, CancellationToken cancellationToken)
    {
        RecurringPayment? payment = await recurringPaymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment is null || payment.UserId != command.UserId)
        {
            return RecurringPaymentErrors.NotFound;
        }

        ErrorOr<Success> result = payment.Resume(command.UserId);
        if (result.IsError)
        {
            return result.FirstError;
        }

        await recurringPaymentRepository.UpdateAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
