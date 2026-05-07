namespace BankingApp.Domain.Aggregates.TransferAggregate;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using BankingApp.Domain.ValueObjects;

public sealed class Transfer : AggregateRoot<int>
{
    private const decimal TwoFaAmountThreshold = 1000m;

    private Transfer()
    {
    }

    public int UserId { get; private set; }

    public int SourceAccountId { get; private set; }

    public int? LedgerTransactionId { get; private set; }

    public string RecipientName { get; private set; } = string.Empty;

    public Iban RecipientIban { get; private set; } = default!;

    public string? RecipientBankName { get; private set; }

    public Money Amount { get; private set; } = default!;

    public decimal? ConvertedAmount { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public decimal Fee { get; private set; }

    public string? Reference { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTime? EstimatedArrival { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Transfer Create(
        int userId,
        int sourceAccountId,
        string recipientName,
        Iban recipientIban,
        Money amount,
        decimal fee,
        string? reference,
        DateTime createdAt)
    {
        return new Transfer
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            RecipientName = recipientName,
            RecipientIban = recipientIban,
            Amount = amount,
            Fee = fee,
            Reference = reference,
            Status = TransferStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public static bool RequiresTwoFactorAuthentication(decimal amount) => amount >= TwoFaAmountThreshold;

    public void MarkExecuted(int? ledgerTransactionId, DateTime? estimatedArrival)
    {
        LedgerTransactionId = ledgerTransactionId;
        EstimatedArrival = estimatedArrival;
        Status = TransferStatus.Completed;
    }

    public void MarkFailed()
    {
        Status = TransferStatus.Failed;
    }
}
