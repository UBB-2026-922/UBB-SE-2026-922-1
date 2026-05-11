namespace BankingApp.Application.Tests.Features.PasswordReset.Queries;

public sealed class VerifyResetTokenQueryTests
{
    [Fact]
    public void Handle_WhenTokenNotFound_ShouldReturnTokenInvalidError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenTokenAlreadyUsed_ShouldReturnTokenAlreadyUsedError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenTokenIsExpired_ShouldReturnTokenExpiredError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenTokenIsValid_ShouldReturnSuccess()
    {
        throw new NotImplementedException();
    }
}
