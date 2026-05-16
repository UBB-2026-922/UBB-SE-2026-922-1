namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Common.Contracts;
using BankingApp.Application.Features.Beneficiaries.Commands;
using BankingApp.Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Repositories;
using BankingApp.Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Moq;
using Xunit;

using MockFactory = BankingApp.Application.Tests.MockFactory;

public sealed class UpdateBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestBeneficiaryId = 100;
    private const string ValidIban = "RO12BANK1234567890123456";
    private const string NewValidIban = "RO99BANK0000000000000000";

    [Fact]
    public async Task Handle_WhenBeneficiaryNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Beneficiary?)null);

        var handler = new UpdateBeneficiaryCommandHandler(
            repositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object);

        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryBelongsToDifferentUser_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        var otherUserBeneficiary = Beneficiary.Create(
            OtherUserId, 
            "Jane Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUserBeneficiary);

        var handler = new UpdateBeneficiaryCommandHandler(
            repositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object);

        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        var handler = new UpdateBeneficiaryCommandHandler(
            repositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object);

        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", "invalid-iban", "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateBeneficiary()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        var handler = new UpdateBeneficiaryCommandHandler(
            repositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object);

        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", NewValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        
        beneficiary.Name.Should().Be(command.Name);
        beneficiary.Iban.Value.Should().Be(command.Iban);
        beneficiary.BankName.Should().Be(command.BankName);

        repositoryMock.Verify(repo => repo.UpdateAsync(beneficiary, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        Mock<IUnitOfWork> unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        
        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        var handler = new UpdateBeneficiaryCommandHandler(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        var command = new UpdateBeneficiaryCommand(TestUserId, TestBeneficiaryId, "New Name", NewValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}