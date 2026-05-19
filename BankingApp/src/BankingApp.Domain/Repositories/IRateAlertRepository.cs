namespace BankingApp.Domain.Repositories;

using Aggregates.RateAlertAggregate;

public interface IRateAlertRepository
{
    public Task<RateAlert?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<RateAlert>> ListByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<RateAlert>> ListAllUntriggeredAsync(CancellationToken cancellationToken = default);
    public Task AddAsync(RateAlert rateAlert, CancellationToken cancellationToken = default);
    public Task UpdateAsync(RateAlert rateAlert, CancellationToken cancellationToken = default);
    public Task DeleteAsync(RateAlert rateAlert, CancellationToken cancellationToken = default);
}
