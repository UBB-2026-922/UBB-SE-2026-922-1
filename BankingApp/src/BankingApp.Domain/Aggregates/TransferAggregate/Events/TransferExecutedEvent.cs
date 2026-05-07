namespace BankingApp.Domain.Aggregates.TransferAggregate.Events;

using BankingApp.Domain.Common;

public record TransferExecutedEvent(int TransferId, DateTime OccurredOnUtc) : IDomainEvent;
