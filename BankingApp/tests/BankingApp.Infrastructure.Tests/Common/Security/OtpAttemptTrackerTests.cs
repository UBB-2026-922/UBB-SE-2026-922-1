namespace BankingApp.Infrastructure.Tests.Common.Security;

public sealed class OtpAttemptTrackerTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void RecordFailure_WhenFirstFailureForUser_ShouldReturnOne()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void RecordFailure_WhenMultipleFailuresForSameUser_ShouldIncrementCount()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void RecordFailure_WhenDifferentUsers_ShouldTrackCountsIndependently()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Reset_WhenCalled_ShouldClearFailureCountForUser()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void RecordFailure_AfterReset_ShouldStartCountFromOne()
    {
        throw new NotImplementedException();
    }
}
