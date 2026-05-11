namespace BankingApp.Infrastructure.Persistence;

using BankingApp.Application.Common.Contracts;
using BankingApp.Domain.Common;
using BankingApp.Domain.Common.Primitives;
using MediatR;

public sealed class UnitOfWork(AppDbContext dbContext, IPublisher publisher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = dbContext.ChangeTracker
            .Entries<AggregateRoot<int>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (AggregateRoot<int> aggregate in aggregates)
        {
            foreach (IDomainEvent domainEvent in aggregate.DomainEvents)
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
