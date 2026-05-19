namespace BankingApp.Infrastructure.Common.Clock;

using Application;
using Application.Shared.Clock;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
