namespace BankingApp.Domain.Common;

using MediatR;

public interface IDomainEvent : INotification
{
    public DateTime OccurredOnUtc { get; }
}