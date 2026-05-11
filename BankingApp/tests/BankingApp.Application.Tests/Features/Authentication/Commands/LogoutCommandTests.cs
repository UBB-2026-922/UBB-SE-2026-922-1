namespace BankingApp.Application.Tests.Features.Authentication.Commands;

public sealed class LogoutCommandTests
{
    [Fact]
    public void Handle_WhenIdentityNotFound_ShouldReturnSessionNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenSessionTokenNotFound_ShouldReturnSessionNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValidSession_ShouldRevokeSessionAndSaveChanges()
    {
        throw new NotImplementedException();
    }
}
