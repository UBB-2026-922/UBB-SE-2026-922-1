namespace BankingApp.Domain.Aggregates.ForexAggregate.Entities;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.ValueObjects;

public sealed class LockedRate : Entity<int>
{
    private const int LockDurationSeconds = 30;

    private LockedRate()
    {
    }

    public int UserId { get; private set; }

    public ExchangeRate ExchangeRate { get; private set; } = default!;

    public DateTime LockedAt { get; private set; }

    public static LockedRate Create(int userId, ExchangeRate exchangeRate, DateTime lockedAt)
    {
        return new LockedRate
        {
            UserId = userId,
            ExchangeRate = exchangeRate,
            LockedAt = lockedAt
        };
    }

    public bool IsExpired() => DateTime.UtcNow - LockedAt > TimeSpan.FromSeconds(LockDurationSeconds);
}
