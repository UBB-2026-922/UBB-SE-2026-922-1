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

public sealed class DeleteBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const int OtherUserId = 2;
    private const int TestBeneficiaryId = 100;
    private const string ValidIban = "RO12BANK1234567890123456";

    [Fact]
    public async Task Handle_WhenBeneficiaryNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Beneficiary?)null);

        DeleteBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        DeleteBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId);

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

        DeleteBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        DeleteBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId);

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
    public async Task Handle_WhenBeneficiaryIsValid_ShouldDeleteBeneficiaryAndSaveChanges()
    {
        // Arrange
        Mock<IBeneficiaryRepository> repositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        var validBeneficiary = Beneficiary.Create(
            TestUserId, 
            "John Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(TestBeneficiaryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validBeneficiary);

        repositoryMock
            .Setup(repo => repo.DeleteAsync(validBeneficiary, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DeleteBeneficiaryCommandHandler handler = new(
            repositoryMock.Object,
            unitOfWorkMock.Object);

        DeleteBeneficiaryCommand command = new(TestUserId, TestBeneficiaryId);

        // Act
        ErrorOr<Success> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Success);

        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
    }
}