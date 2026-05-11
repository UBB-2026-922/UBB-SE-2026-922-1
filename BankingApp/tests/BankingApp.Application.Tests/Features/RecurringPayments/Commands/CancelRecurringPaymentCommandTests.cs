namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

public sealed class CancelRecurringPaymentCommandTests
{
    [Fact]
    public void Handle_WhenPaymentNotFound_ShouldReturnNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenCalledByNonOwner_ShouldReturnForbiddenError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldCancelPaymentAndSaveChanges()
    {
        throw new NotImplementedException();
    }
}
