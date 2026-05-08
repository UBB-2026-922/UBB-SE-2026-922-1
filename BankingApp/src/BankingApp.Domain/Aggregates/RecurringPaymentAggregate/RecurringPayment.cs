namespace BankingApp.Domain.Aggregates.RecurringPaymentAggregate;

using Common.Errors;
using Common.Extensions;
using Common.Primitives;
using Enums;
using ErrorOr;

public sealed class RecurringPayment : AggregateRoot<int>
{
    private RecurringPayment()
    {
    }

    public int UserId { get; private set; }

    public int BillerId { get; private set; }

    public int SourceAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public bool IsPayInFull { get; private set; }

    public RecurringFrequency Frequency { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime? EndDate { get; private set; }

    public DateTime NextExecutionDate { get; private set; }

    public RecurringPaymentStatus Status { get; private set; } = RecurringPaymentStatus.Active;

    public DateTime CreatedAt { get; private set; }

    public static ErrorOr<RecurringPayment> Create(
        int userId,
        int billerId,
        int sourceAccountId,
        decimal amount,
        bool isPayInFull,
        RecurringFrequency frequency,
        DateTime startDate,
        DateTime? endDate,
        DateTime createdAt)
    {
        if (amount <= 0)
        {
            return RecurringPaymentErrors.InvalidAmount;
        }

        if (endDate.HasValue && endDate.Value <= startDate)
        {
            return RecurringPaymentErrors.InvalidEndDate;
        }

        return new RecurringPayment
        {
            UserId = userId,
            BillerId = billerId,
            SourceAccountId = sourceAccountId,
            Amount = amount,
            IsPayInFull = isPayInFull,
            Frequency = frequency,
            StartDate = startDate,
            EndDate = endDate,
            NextExecutionDate = frequency.AdvanceFrom(startDate),
            CreatedAt = createdAt
        };
    }

    public ErrorOr<Success> Pause(int userId)
    {
        if (userId != UserId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        Status = RecurringPaymentStatus.Paused;
        return Result.Success;
    }

    public ErrorOr<Success> Resume(int userId)
    {
        if (userId != UserId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        if (Status != RecurringPaymentStatus.Paused)
        {
            return RecurringPaymentErrors.ResumeConflict;
        }

        Status = RecurringPaymentStatus.Active;
        return Result.Success;
    }

    public void AdvanceAfterSuccessfulExecution()
    {
        DateTime nextExecutionDate = Frequency.AdvanceFrom(NextExecutionDate);
        if (EndDate.HasValue && nextExecutionDate > EndDate.Value)
        {
            Status = RecurringPaymentStatus.Cancelled;
            return;
        }

        NextExecutionDate = nextExecutionDate;
    }

    public ErrorOr<Success> Cancel(int userId)
    {
        if (userId != UserId)
        {
            return RecurringPaymentErrors.Forbidden;
        }

        Status = RecurringPaymentStatus.Cancelled;
        return Result.Success;
    }
}
