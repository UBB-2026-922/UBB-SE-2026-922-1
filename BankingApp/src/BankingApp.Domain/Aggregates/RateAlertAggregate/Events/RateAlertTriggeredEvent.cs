namespace BankingApp.Domain.Aggregates.RateAlertAggregate.Events;

using Common;

public record RateAlertTriggeredEvent(int RateAlertId, DateTime OccurredOnUtc) : IDomainEvent;
