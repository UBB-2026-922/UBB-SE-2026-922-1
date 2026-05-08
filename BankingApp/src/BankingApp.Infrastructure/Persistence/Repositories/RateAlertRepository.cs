namespace BankingApp.Infrastructure.Persistence.Repositories;

using BankingApp.Domain.Aggregates.RateAlertAggregate;
using BankingApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class RateAlertRepository(AppDbContext dbContext) : IRateAlertRepository
{
    public async Task<RateAlert?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RateAlerts.FirstOrDefaultAsync(rateAlert => rateAlert.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RateAlert>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.RateAlerts
            .Where(rateAlert => rateAlert.UserId == userId)
            .OrderByDescending(rateAlert => rateAlert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RateAlert>> ListAllUntriggeredAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.RateAlerts
            .Where(rateAlert => !rateAlert.IsTriggered)
            .OrderBy(rateAlert => rateAlert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RateAlert rateAlert, CancellationToken cancellationToken = default)
    {
        await dbContext.RateAlerts.AddAsync(rateAlert, cancellationToken);
    }

    public Task UpdateAsync(RateAlert rateAlert, CancellationToken cancellationToken = default)
    {
        dbContext.RateAlerts.Update(rateAlert);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RateAlert rateAlert, CancellationToken cancellationToken = default)
    {
        dbContext.RateAlerts.Remove(rateAlert);
        return Task.CompletedTask;
    }
}
