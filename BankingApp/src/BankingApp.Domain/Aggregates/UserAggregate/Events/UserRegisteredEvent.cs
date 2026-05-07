namespace BankingApp.Domain.Aggregates.UserAggregate.Events;

using BankingApp.Domain.Common;

public record UserRegisteredEvent(int UserId, DateTime OccurredOnUtc) : IDomainEvent;
