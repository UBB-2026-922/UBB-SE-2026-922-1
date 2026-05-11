namespace BankingApp.Application.Tests.Features.Authentication.Commands;

public sealed class LoginCommandTests
{
    [Fact]
    public void Handle_WhenEmailFormatIsInvalid_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenUserNotFound_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenIdentityNotFound_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenAccountIsCurrentlyLocked_ShouldReturnAccountLockedError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenLockoutHasExpired_ShouldResetFailedAttemptsAndProceed()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordHashIsNull_ShouldReturnInvalidCredentialsError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordVerificationFails_ShouldIncrementFailedAttempts()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenFailedAttemptsReachMaximum_ShouldLockAccountAndReturnLockedError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordCorrectAndNo2Fa_ShouldOpenSessionAndReturnToken()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordCorrectAnd2FaEnabled_ShouldReturnRequiresTwoFactor()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordCorrectAnd2FaMethodIsEmail_ShouldSendOtpByEmail()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenPasswordCorrectAnd2FaMethodIsAuthenticator_ShouldGenerateTotp()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenLoginSucceeds_ShouldResetFailedAttempts()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenLoginSucceeds_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
