namespace BankingApp.Domain.Aggregates.ForexAggregate.Events;

using BankingApp.Domain.Common;

public record ForexTransactionExecutedEvent(int ForexTransactionId, DateTime OccurredOnUtc) : IDomainEvent;
