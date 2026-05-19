namespace BankingApp.Domain.Aggregates.RecurringPaymentAggregate.Events;

using Common;

public record RecurringPaymentsExecutedEvent(int RecurringPaymentId, DateTime OccurredOnUtc) : IDomainEvent;
