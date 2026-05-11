namespace BankingApp.Domain.Aggregates.IdentityAggregate.Events;

using Common;

public record PasswordResetRequestedEvent(int IdentityAccountId, DateTime OccurredOnUtc) : IDomainEvent;
