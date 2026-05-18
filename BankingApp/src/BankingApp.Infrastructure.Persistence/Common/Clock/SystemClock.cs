namespace BankingApp.Infrastructure.Common.Clock;

using Application;
using BankingApp.Application.Common.Utilities;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
