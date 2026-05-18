namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

using BankingApp.Application.Features.Beneficiaries.Commands;
using Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;

public sealed class UpdateBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestBeneficiaryId = 100;
    private const string ValidIban = "RO12BANK1234567890123456";
    private const string NewValidIban = "RO99BANK0000000000000000";

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenBeneficiaryNotFound_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _beneficiaryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken))
            .ReturnsAsync((Beneficiary?)null);

        UpdateBeneficiaryCommandHandler handler = CreateHandler();
        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);

        _beneficiaryRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryBelongsToDifferentUser_ShouldReturnNotFoundErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Beneficiary otherUserBeneficiary = CreateBeneficiary(OtherUserId);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken))
            .ReturnsAsync(otherUserBeneficiary);

        UpdateBeneficiaryCommandHandler handler = CreateHandler();
        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);

        _beneficiaryRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanErrorAndNotPersist()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Beneficiary beneficiary = CreateBeneficiary(TestUserId);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken))
            .ReturnsAsync(beneficiary);

        UpdateBeneficiaryCommandHandler handler = CreateHandler();
        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", "invalid-iban", "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);

        _beneficiaryRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldUpdateTrimmedBeneficiaryAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Beneficiary beneficiary = CreateBeneficiary(TestUserId);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken))
            .ReturnsAsync(beneficiary);

        _beneficiaryRepositoryMock
            .Setup(repository => repository.UpdateAsync(beneficiary, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        UpdateBeneficiaryCommandHandler handler = CreateHandler();
        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "  New Name  ", NewValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        beneficiary.Name.Should().Be("New Name");
        beneficiary.Iban.Value.Should().Be(NewValidIban);
        beneficiary.BankName.Should().Be(command.BankName);

        _beneficiaryRepositoryMock.Verify(repository => repository.GetByIdAsync(TestBeneficiaryId, cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.Verify(repository => repository.UpdateAsync(beneficiary, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _beneficiaryRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private UpdateBeneficiaryCommandHandler CreateHandler()
    {
        return new UpdateBeneficiaryCommandHandler(_beneficiaryRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static Beneficiary CreateBeneficiary(int userId)
    {
        return Beneficiary.Create(userId, "John Doe", Iban.Create(ValidIban).Value, null, _testNow);
    }
}
