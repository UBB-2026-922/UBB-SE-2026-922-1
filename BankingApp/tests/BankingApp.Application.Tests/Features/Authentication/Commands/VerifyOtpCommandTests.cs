namespace BankingApp.Application.Tests.Features.Authentication.Commands;

public sealed class VerifyOtpCommandTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenIdentityNotFound_ShouldReturnUserNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenOtpAttemptsExceeded_ShouldReturnOtpAttemptsExceededError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenOtpIsInvalid_ShouldRecordFailureAndReturnInvalidOtpError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenOtpIsValid_ShouldResetAttemptsAndOpenSession()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenOtpIsValid_ShouldReturnTokenAndSaveChanges()
    {
        throw new NotImplementedException();
    }
}
