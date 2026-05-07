namespace BankingApp.Domain.Aggregates.ForexAggregate;

using Common.Primitives;
using Enums;
using Money = NodaMoney.Money;

public sealed class ForexTransaction : AggregateRoot<int>
{
    private ForexTransaction()
    {
    }

    public int UserId { get; private set; }

    public int SourceAccountId { get; private set; }

    public int TargetAccountId { get; private set; }

    public int? SourceLedgerTransactionId { get; private set; }

    public int? TargetLedgerTransactionId { get; private set; }

    public Money SourceAmount { get; private set; } = default!;

    public Money TargetAmount { get; private set; } = default!;

    public decimal ExchangeRate { get; private set; }

    public Money Commission { get; private set; } = default!;

    public DateTime? RateLockedAt { get; private set; }

    public ExchangeTransactionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static ForexTransaction Create(
        int userId,
        int sourceAccountId,
        int targetAccountId,
        Money sourceAmount,
        Money targetAmount,
        decimal exchangeRate,
        Money commission,
        DateTime createdAt)
    {
        return new ForexTransaction
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            SourceAmount = sourceAmount,
            TargetAmount = targetAmount,
            ExchangeRate = exchangeRate,
            Commission = commission,
            Status = ExchangeTransactionStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public void LockRate(DateTime lockedAt)
    {
        RateLockedAt = lockedAt;
    }

    public void MarkExecuted(int? sourceLedgerTransactionId, int? targetLedgerTransactionId)
    {
        SourceLedgerTransactionId = sourceLedgerTransactionId;
        TargetLedgerTransactionId = targetLedgerTransactionId;
        Status = ExchangeTransactionStatus.Completed;
    }
}
