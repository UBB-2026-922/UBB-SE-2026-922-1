namespace BankingApp.Infrastructure.Persistence.Repositories;

using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Enums;
using BankingApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class RecurringPaymentRepository(AppDbContext dbContext) : IRecurringPaymentRepository
{
    public async Task<RecurringPayment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RecurringPayments.FirstOrDefaultAsync(recurringPayment => recurringPayment.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RecurringPayment>> ListDueAsync(DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        return await dbContext.RecurringPayments
            .Where(recurringPayment =>
                recurringPayment.Status == RecurringPaymentStatus.Active &&
                recurringPayment.NextExecutionDate <= asOfUtc)
            .OrderBy(recurringPayment => recurringPayment.NextExecutionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RecurringPayment>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.RecurringPayments
            .Where(recurringPayment => recurringPayment.UserId == userId)
            .OrderByDescending(recurringPayment => recurringPayment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default)
    {
        await dbContext.RecurringPayments.AddAsync(recurringPayment, cancellationToken);
    }

    public Task UpdateAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default)
    {
        dbContext.RecurringPayments.Update(recurringPayment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RecurringPayment recurringPayment, CancellationToken cancellationToken = default)
    {
        dbContext.RecurringPayments.Remove(recurringPayment);
        return Task.CompletedTask;
    }
}
