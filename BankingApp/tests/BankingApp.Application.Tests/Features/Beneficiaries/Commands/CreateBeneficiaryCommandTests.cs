namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

public sealed class CreateBeneficiaryCommandTests
{
    [Fact]
    public void Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenBeneficiaryWithSameIbanAlreadyExists_ShouldReturnDuplicateError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldCreateAndPersistBeneficiary()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
