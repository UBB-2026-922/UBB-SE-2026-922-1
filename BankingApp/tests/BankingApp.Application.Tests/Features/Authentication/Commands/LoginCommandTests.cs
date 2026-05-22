namespace BankingApp.Application.Tests.Features.Authentication.Commands;

public sealed class LoginCommandTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenEmailFormatIsInvalid_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenUserNotFound_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenIdentityNotFound_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAccountIsCurrentlyLocked_ShouldReturnAccountLockedError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenLockoutHasExpired_ShouldResetFailedAttemptsAndProceed()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenPasswordHashIsNull_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenPasswordVerificationFails_ShouldIncrementFailedAttempts()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenFailedAttemptsReachMaximum_ShouldLockAccountAndReturnLockedError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenPasswordCorrect_ShouldOpenSessionAndReturnToken()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenLoginSucceeds_ShouldResetFailedAttempts()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenLoginSucceeds_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
