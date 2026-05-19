namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Enums;
using Domain.Repositories;
using BillPayments;
using ErrorOr;
using MediatR;
using Shared.Clock;
using Shared.Persistence;
using Money = NodaMoney.Money;

public sealed record ProcessDueRecurringPaymentsCommand : IRequest<ErrorOr<int>>;

public sealed class ProcessDueRecurringPaymentsCommandHandler(
    IRecurringPaymentRepository recurringPaymentRepository,
    IAccountRepository accountRepository,
    IBillPaymentRepository billPaymentRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IRequestHandler<ProcessDueRecurringPaymentsCommand, ErrorOr<int>>
{
    public async Task<ErrorOr<int>> Handle(ProcessDueRecurringPaymentsCommand command, CancellationToken cancellationToken)
    {
        DateTime now = clock.UtcNow;
        IReadOnlyCollection<RecurringPayment> duePayments = await recurringPaymentRepository.ListDueAsync(now, cancellationToken);
        int processed = 0;

        foreach (RecurringPayment recurring in duePayments)
        {
            Account? account = await accountRepository.GetByIdAsync(recurring.SourceAccountId, cancellationToken);
            if (account is null || !account.IsActive())
            {
                continue;
            }

            Money amount = new(recurring.Amount, account.Balance.Currency);
            Money fee = BillPaymentFeePolicy.Calculate(recurring.Amount, account.Balance.Currency);

            ErrorOr<BillPayment> paymentResult = BillPayment.Create(
                recurring.UserId,
                recurring.SourceAccountId,
                recurring.BillerId,
                $"RECURRING-{recurring.Id}",
                amount,
                fee,
                now);

            if (paymentResult.IsError)
            {
                continue;
            }

            BillPayment payment = paymentResult.Value;
            string receiptNumber = $"RCP-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

            ErrorOr<Money> newBalanceResult = account.Debit(payment.TotalDebit, now);
            if (newBalanceResult.IsError)
            {
                continue;
            }

            Transaction txn = account.RecordTransaction(
                receiptNumber,
                "RECURRING_PAYMENT",
                TransactionDirection.Out,
                payment.Amount,
                newBalanceResult.Value,
                TransactionStatus.Completed,
                now);

            payment.MarkProcessed(receiptNumber, txn.Id);

            await accountRepository.UpdateAsync(account, cancellationToken);
            await billPaymentRepository.AddAsync(payment, cancellationToken);

            recurring.AdvanceAfterSuccessfulExecution();
            await recurringPaymentRepository.UpdateAsync(recurring, cancellationToken);

            processed++;
        }

        if (processed > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }
}
