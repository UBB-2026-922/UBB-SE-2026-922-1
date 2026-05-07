namespace BankingApp.Domain.Aggregates.AccountAggregate.Events;

using BankingApp.Domain.Common;

public sealed record BalanceUpdatedEvent(
    int AccountId,
    decimal OldBalance,
    decimal NewBalance,
    DateTime OccurredOnUtc) : IDomainEvent;
