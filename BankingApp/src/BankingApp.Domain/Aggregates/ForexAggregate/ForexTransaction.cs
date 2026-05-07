namespace BankingApp.Domain.Aggregates.ForexAggregate;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;

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

    public string SourceCurrency { get; private set; } = string.Empty;

    public string TargetCurrency { get; private set; } = string.Empty;

    public decimal SourceAmount { get; private set; }

    public decimal TargetAmount { get; private set; }

    public decimal ExchangeRate { get; private set; }

    public decimal Commission { get; private set; }

    public DateTime? RateLockedAt { get; private set; }

    public ExchangeTransactionStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static ForexTransaction Create(
        int userId,
        int sourceAccountId,
        int targetAccountId,
        string sourceCurrency,
        string targetCurrency,
        decimal sourceAmount,
        decimal targetAmount,
        decimal exchangeRate,
        decimal commission,
        DateTime createdAt)
    {
        return new ForexTransaction
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            SourceCurrency = sourceCurrency,
            TargetCurrency = targetCurrency,
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
