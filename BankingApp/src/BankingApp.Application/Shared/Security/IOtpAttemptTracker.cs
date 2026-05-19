namespace BankingApp.Application.Common.Security;

public interface IOtpAttemptTracker
{
    public int RecordFailure(int userId);
    public void Reset(int userId);
}
