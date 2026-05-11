namespace BankingApp.Domain.Tests.Aggregates.RecurringPaymentAggregate;

public sealed class RecurringPaymentTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenAmountIsZero_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenAmountIsNegative_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenEndDateIsNotAfterStartDate_ShouldReturnInvalidEndDateError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenEndDateEqualsStartDate_ShouldReturnInvalidEndDateError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Create_WhenValidParams_ShouldCreateWithActiveStatus()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Pause_WhenCalledByOwner_ShouldSetStatusToPaused()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Pause_WhenCalledByNonOwner_ShouldReturnForbiddenError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Resume_WhenPausedAndCalledByOwner_ShouldSetStatusToActive()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Resume_WhenCalledByNonOwner_ShouldReturnForbiddenError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Resume_WhenStatusIsNotPaused_ShouldReturnResumeConflictError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Cancel_WhenCalledByOwner_ShouldSetStatusToCancelled()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Cancel_WhenCalledByNonOwner_ShouldReturnForbiddenError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void AdvanceAfterSuccessfulExecution_WhenNextDateIsBeforeEndDate_ShouldAdvanceNextExecutionDate()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void AdvanceAfterSuccessfulExecution_WhenNextDateExceedsEndDate_ShouldSetStatusToCancelled()
    {
        throw new NotImplementedException();
    }
}
