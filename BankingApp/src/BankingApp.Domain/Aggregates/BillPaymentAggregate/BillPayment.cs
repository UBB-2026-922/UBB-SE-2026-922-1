namespace BankingApp.Domain.Aggregates.BillPaymentAggregate;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;

public sealed class BillPayment : AggregateRoot<int>
{
    private BillPayment()
    {
    }

    public int UserId { get; private set; }

    public int SourceAccountId { get; private set; }

    public int BillerId { get; private set; }

    public int? LedgerTransactionId { get; private set; }

    public string BillerReference { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public decimal Fee { get; private set; }

    public string ReceiptNumber { get; private set; } = string.Empty;

    public BillPaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static ErrorOr<BillPayment> Create(
        int userId,
        int sourceAccountId,
        int billerId,
        string billerReference,
        decimal amount,
        decimal fee,
        DateTime createdAt)
    {
        if (amount <= 0)
        {
            return BillPaymentErrors.InvalidAmount;
        }

        return new BillPayment
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            BillerId = billerId,
            BillerReference = billerReference,
            Amount = amount,
            Fee = fee,
            Status = BillPaymentStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public void MarkProcessed(string receiptNumber, int? ledgerTransactionId)
    {
        ReceiptNumber = receiptNumber;
        LedgerTransactionId = ledgerTransactionId;
        Status = BillPaymentStatus.Completed;
    }
}
