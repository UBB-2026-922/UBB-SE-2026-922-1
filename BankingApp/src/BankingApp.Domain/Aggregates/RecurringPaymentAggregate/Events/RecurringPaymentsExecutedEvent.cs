namespace BankingApp.Domain.Aggregates.RecurringPaymentAggregate.Events;

using BankingApp.Domain.Common;

public record RecurringPaymentsExecutedEvent(int RecurringPaymentId, DateTime OccurredOnUtc) : IDomainEvent;
