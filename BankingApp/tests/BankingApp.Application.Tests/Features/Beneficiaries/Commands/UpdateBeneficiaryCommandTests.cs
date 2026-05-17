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
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Beneficiary?)null);

        UpdateBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        UpdateBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryBelongsToDifferentUser_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        var otherUserBeneficiary = Beneficiary.Create(
            OtherUserId, 
            "Jane Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUserBeneficiary);

        UpdateBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        UpdateBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId, "New Name", ValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.NotFound);

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        UpdateBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        UpdateBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId, "New Name", "invalid-iban", "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldUpdateBeneficiary()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        repositoryMock
            .Setup(repo => repo.UpdateAsync(beneficiary, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UpdateBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        UpdateBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId, "New Name", NewValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);
        
        beneficiary.Name.Should().Be(command.Name);
        beneficiary.Iban.Value.Should().Be(command.Iban);
        beneficiary.BankName.Should().Be(command.BankName);

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldSaveChanges()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        var beneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);

        repositoryMock
            .Setup(repo => repo.UpdateAsync(beneficiary, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        UpdateBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        UpdateBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId, "New Name", NewValidIban, "New Bank");

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }
}