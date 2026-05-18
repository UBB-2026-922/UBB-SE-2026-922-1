namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Contracts.Features.RecurringPayments.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record CreateRecurringPaymentCommand(
    int UserId,
    int BillerId,
    int SourceAccountId,
    decimal Amount,
    bool IsPayInFull,
    RecurringFrequency Frequency,
    DateTime StartDate,
    DateTime? EndDate)
    : IRequest<ErrorOr<RecurringPaymentResponse>>;

public sealed class CreateRecurringPaymentCommandHandler(
    IBillerRepository billerRepository,
    IAccountRepository accountRepository,
    IRecurringPaymentRepository recurringPaymentRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IRequestHandler<CreateRecurringPaymentCommand, ErrorOr<RecurringPaymentResponse>>
{
    public async Task<ErrorOr<RecurringPaymentResponse>> Handle(CreateRecurringPaymentCommand command, CancellationToken cancellationToken)
    {
        Biller? biller = await billerRepository.GetByIdAsync(command.BillerId, cancellationToken);
        if (biller is null)
        {
            return BillerErrors.BillerNotFound;
        }

        Account? account = await accountRepository.GetByIdAsync(command.SourceAccountId, cancellationToken);
        if (account is null || account.UserId != command.UserId)
        {
            return AccountErrors.NotFound;
        }

        if (!account.IsActive())
        {
            return AccountErrors.NotActive;
        }

        ErrorOr<RecurringPayment> paymentResult = RecurringPayment.Create(
            command.UserId,
            command.BillerId,
            command.SourceAccountId,
            command.Amount,
            command.IsPayInFull,
            command.Frequency,
            command.StartDate,
            command.EndDate,
            clock.UtcNow);

        if (paymentResult.IsError)
        {
            return paymentResult.FirstError;
        }

        RecurringPayment payment = paymentResult.Value;
        await recurringPaymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecurringPaymentResponse
        {
            Id = payment.Id,
            UserId = payment.UserId,
            BillerId = payment.BillerId,
            SourceAccountId = payment.SourceAccountId,
            Amount = payment.Amount,
            IsPayInFull = payment.IsPayInFull,
            Frequency = payment.Frequency,
            StartDate = payment.StartDate,
            EndDate = payment.EndDate,
            NextExecutionDate = payment.NextExecutionDate,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }
}
