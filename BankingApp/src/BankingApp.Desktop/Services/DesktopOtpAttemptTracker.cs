namespace BankingApp.Desktop.Services;

using System.Collections.Concurrent;
using Application.Services.Login;

internal sealed class DesktopOtpAttemptTracker : IOtpAttemptTracker
{
    private readonly ConcurrentDictionary<int, int> _failedAttempts = new();

    public int RecordFailure(int userId)
        => _failedAttempts.AddOrUpdate(userId, 1, (_, count) => count + 1);

    public void Reset(int userId)
        => _failedAttempts.TryRemove(userId, out _);
}
