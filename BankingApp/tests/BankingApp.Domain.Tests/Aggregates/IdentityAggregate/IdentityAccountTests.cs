namespace BankingApp.Domain.Tests.Aggregates.IdentityAggregate;

public sealed class IdentityAccountTests
{
    [Fact]
    public void IsCurrentlyLocked_WhenLockedWithFutureLockoutEnd_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IsCurrentlyLocked_WhenLockedWithPastLockoutEnd_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IsCurrentlyLocked_WhenNotLocked_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IncrementFailedAttempts_WhenCalled_ShouldIncrementCounter()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void LockAccount_WhenCalled_ShouldSetIsLockedToTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void LockAccount_WhenCalled_ShouldSetLockoutEnd()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ResetFailedAttempts_WhenCalled_ShouldSetCounterToZero()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ResetFailedAttempts_WhenCalled_ShouldUnlockAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void UpdatePassword_WhenCalled_ShouldReplacePasswordHash()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void TryRevokeSession_WhenSessionExists_ShouldRevokeItAndReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void TryRevokeSession_WhenSessionDoesNotExist_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void InvalidateAllSessions_WhenCalled_ShouldRevokeAllNonRevokedSessions()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void InvalidateAllSessions_WhenCalled_ShouldNotAffectAlreadyRevokedSessions()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Enable2Fa_WhenCalled_ShouldSetIs2FaEnabledToTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Enable2Fa_WhenCalled_ShouldSetPreferred2FaMethod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Disable2Fa_WhenCalled_ShouldSetIs2FaEnabledToFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Disable2Fa_WhenCalled_ShouldClearPreferred2FaMethod()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void OpenSession_WhenCalled_ShouldAddSessionToCollection()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IssuePasswordResetToken_WhenCalled_ShouldAddTokenToCollection()
    {
        throw new NotImplementedException();
    }
}
