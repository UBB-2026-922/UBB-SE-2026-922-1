namespace BankingApp.Domain.Aggregates.IdentityAggregate.Events;

using BankingApp.Domain.Common;

public record UserLoggedInEvent(int IdentityAccountId, DateTime OccurredOnUtc) : IDomainEvent;
