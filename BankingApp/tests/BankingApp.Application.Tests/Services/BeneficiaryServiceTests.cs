namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Beneficiaries.Commands;
using BankingApp.Application.Features.Beneficiaries.Dtos;
using BankingApp.Application.Features.Beneficiaries.Queries;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class GetBeneficiariesQueryHandlerTests
{
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepo = MockFactory.CreateBeneficiaryRepository();

    private GetBeneficiariesQueryHandler CreateHandler() => new(_beneficiaryRepo.Object);

    [Fact]
    public async Task Handle_WhenNoBeneficiaries_ReturnsEmptyList()
    {
        var query = new GetBeneficiariesQuery(1);

        ErrorOr<List<BeneficiaryDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenBeneficiariesExist_ReturnsMappedList()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var beneficiary = Beneficiary.Create(1, "John Doe", iban, "Test Bank", DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Beneficiary>)new[] { beneficiary });
        var query = new GetBeneficiariesQuery(1);

        ErrorOr<List<BeneficiaryDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("John Doe");
    }
}

public sealed class CreateBeneficiaryCommandHandlerTests
{
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepo = MockFactory.CreateBeneficiaryRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private CreateBeneficiaryCommandHandler CreateHandler() => new(
        _beneficiaryRepo.Object,
        _unitOfWork.Object,
        _clock.Object,
        NullLogger<CreateBeneficiaryCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenIbanInvalid_ReturnsError()
    {
        var command = new CreateBeneficiaryCommand(1, "John Doe", "INVALID-IBAN", null);

        ErrorOr<BeneficiaryDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIbanAlreadyExists_ReturnsDuplicateError()
    {
        const string ibanValue = "RO49AAAA1B31007593840000";
        var iban = Iban.Create(ibanValue).Value;
        var existing = Beneficiary.Create(1, "Existing", iban, null, DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Beneficiary>)new[] { existing });
        var command = new CreateBeneficiaryCommand(1, "John Doe", ibanValue, null);

        ErrorOr<BeneficiaryDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesBeneficiaryAndSaves()
    {
        var command = new CreateBeneficiaryCommand(1, "John Doe", "RO49AAAA1B31007593840000", "Test Bank");

        ErrorOr<BeneficiaryDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("John Doe");
        _beneficiaryRepo.Verify(r => r.AddAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class UpdateBeneficiaryCommandHandlerTests
{
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepo = MockFactory.CreateBeneficiaryRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private UpdateBeneficiaryCommandHandler CreateHandler() => new(
        _beneficiaryRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenBeneficiaryNotFound_ReturnsNotFoundError()
    {
        var command = new UpdateBeneficiaryCommand(1, 99, "New Name", "RO49AAAA1B31007593840000", null);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryBelongsToDifferentUser_ReturnsNotFoundError()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var beneficiary = Beneficiary.Create(2, "Someone Else", iban, null, DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var command = new UpdateBeneficiaryCommand(1, 10, "New Name", "RO49AAAA1B31007593840000", null);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesAndSaves()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var beneficiary = Beneficiary.Create(1, "Old Name", iban, null, DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var command = new UpdateBeneficiaryCommand(1, 10, "New Name", "RO49AAAA1B31007593840000", "New Bank");

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _beneficiaryRepo.Verify(r => r.UpdateAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class DeleteBeneficiaryCommandHandlerTests
{
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepo = MockFactory.CreateBeneficiaryRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private DeleteBeneficiaryCommandHandler CreateHandler() => new(
        _beneficiaryRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenBeneficiaryNotFound_ReturnsNotFoundError()
    {
        var command = new DeleteBeneficiaryCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryBelongsToDifferentUser_ReturnsNotFoundError()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var beneficiary = Beneficiary.Create(2, "Someone Else", iban, null, DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var command = new DeleteBeneficiaryCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesAndSaves()
    {
        var iban = Iban.Create("RO49AAAA1B31007593840000").Value;
        var beneficiary = Beneficiary.Create(1, "John Doe", iban, null, DateTime.UtcNow);
        _beneficiaryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var command = new DeleteBeneficiaryCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _beneficiaryRepo.Verify(r => r.DeleteAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
