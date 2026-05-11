namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

public sealed class CreateRecurringPaymentCommandTests
{
    [Fact]
    public void Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenDomainValidationFails_ShouldReturnDomainError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldCreateAndPersistRecurringPayment()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
