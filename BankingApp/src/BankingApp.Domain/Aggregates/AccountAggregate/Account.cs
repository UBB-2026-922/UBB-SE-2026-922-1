namespace BankingApp.Domain.Aggregates.AccountAggregate;

using Entities;
using Events;
using Common.Primitives;
using Enums;
using ValueObjects;

public sealed class Account : AggregateRoot<int>
{
    private readonly List<Card> _cards = [];
    private readonly List<Transaction> _transactions = [];

    private Account()
    {
    }

    public int UserId { get; private set; }

    public string? AccountName { get; private set; }

    public Iban Iban { get; private set; } = default!;

    public Currency Currency { get; private set; }

    public decimal Balance { get; private set; }

    public AccountType AccountType { get; private set; }

    public AccountStatus Status { get; private set; } = AccountStatus.Active;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public static Account Open(int userId, Iban iban, Currency currency, AccountType accountType, string? accountName, DateTime createdAt)
    {
        return new Account
        {
            UserId = userId,
            Iban = iban,
            Currency = currency,
            AccountType = accountType,
            AccountName = accountName,
            CreatedAt = createdAt
        };
    }

    public bool IsActive() => Status == AccountStatus.Active;

    public bool HasSufficientFunds(decimal amount) => amount >= 0 && Balance >= amount;

    public void Rename(string? accountName)
    {
        AccountName = accountName;
    }

    public void ChangeBalance(decimal newBalance, DateTime occurredOnUtc)
    {
        decimal oldBalance = Balance;
        Balance = newBalance;
        Raise(new BalanceUpdatedEvent(Id, oldBalance, newBalance, occurredOnUtc));
    }

    public Card IssueCard(
        string cardNumber,
        string cardholderName,
        DateTime expiryDate,
        string cvv,
        CardType cardType,
        string? cardBrand,
        DateTime createdAt)
    {
        var card = Card.Create(Id, UserId, cardNumber, cardholderName, expiryDate, cvv, cardType, cardBrand, createdAt);
        _cards.Add(card);
        return card;
    }

    public Transaction RecordTransaction(
        string transactionRef,
        string type,
        TransactionDirection direction,
        decimal amount,
        decimal balanceAfter,
        TransactionStatus status,
        DateTime createdAt)
    {
        var transaction = Transaction.Create(
            Id,
            transactionRef,
            type,
            direction,
            amount,
            Currency,
            balanceAfter,
            status,
            createdAt);

        _transactions.Add(transaction);
        Raise(new TransactionRecordedEvent(Id, transactionRef, createdAt));
        return transaction;
    }
}
