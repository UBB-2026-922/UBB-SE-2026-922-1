namespace BankingApp.Domain.Aggregates.AccountAggregate.Events;

using BankingApp.Domain.Common;

public sealed record TransactionRecordedEvent(
    int AccountId,
    string TransactionRef,
    DateTime OccurredOnUtc) : IDomainEvent;
