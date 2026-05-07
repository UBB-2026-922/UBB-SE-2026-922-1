namespace BankingApp.Domain.Aggregates.IdentityAggregate.Events;

using BankingApp.Domain.Common;

public record PasswordResetRequestedEvent(int IdentityAccountId, DateTime OccurredOnUtc) : IDomainEvent;
