namespace BankingApp.Domain.Aggregates.BillPaymentAggregate.Events;

using BankingApp.Domain.Common;

public record BillPaymentProcessedEvent(int BillPaymentId, DateTime OccurredOnUtc) : IDomainEvent;
