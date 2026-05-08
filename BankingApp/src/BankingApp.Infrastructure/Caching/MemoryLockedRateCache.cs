namespace BankingApp.Infrastructure.Caching;

using BankingApp.Application.Common.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Currency = NodaMoney.Currency;

public sealed class MemoryLockedRateCache(IMemoryCache memoryCache) : ILockedRateCache
{
    private static readonly TimeSpan _lockTtl = TimeSpan.FromSeconds(30);

    public void Store(int userId, Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTime lockedAt)
    {
        memoryCache.Set(userId, new LockedRate(baseCurrency, quoteCurrency, rate, lockedAt), _lockTtl);
    }

    public LockedRate? TryGet(int userId)
    {
        return memoryCache.TryGetValue(userId, out LockedRate? lockedRate) ? lockedRate : null;
    }

    public void Remove(int userId)
    {
        memoryCache.Remove(userId);
    }
}
