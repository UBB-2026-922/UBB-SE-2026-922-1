namespace BankingApp.Infrastructure.Tests.Common.Security;

public sealed class OtpAttemptTrackerTests
{
    [Fact]
    public void RecordFailure_WhenFirstFailureForUser_ShouldReturnOne()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void RecordFailure_WhenMultipleFailuresForSameUser_ShouldIncrementCount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void RecordFailure_WhenDifferentUsers_ShouldTrackCountsIndependently()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Reset_WhenCalled_ShouldClearFailureCountForUser()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void RecordFailure_AfterReset_ShouldStartCountFromOne()
    {
        throw new NotImplementedException();
    }
}
