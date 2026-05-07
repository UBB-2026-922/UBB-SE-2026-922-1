namespace BankingApp.Domain.Aggregates.RateAlertAggregate.Events;

using BankingApp.Domain.Common;

public record RateAlertTriggeredEvent(int RateAlertId, DateTime OccurredOnUtc) : IDomainEvent;
