namespace BankingApp.Application.Tests.Features.RecurringPayments.Commands;

public sealed class ProcessDueRecurringPaymentsCommandTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenNoDuePayments_ShouldReturnSuccessWithoutProcessing()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenDuePaymentProcessedSuccessfully_ShouldAdvanceNextExecutionDate()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenDuePaymentHasNoBiller_ShouldSkipPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenDuePaymentHasNoAccount_ShouldSkipPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenDuePaymentHasInsufficientFunds_ShouldSkipPayment()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenPaymentIsProcessed_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
