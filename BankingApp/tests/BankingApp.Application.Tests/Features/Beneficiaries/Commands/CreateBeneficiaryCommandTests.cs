namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Common.Contracts;
using BankingApp.Application.Features.Beneficiaries.Commands;
using BankingApp.Application.Features.Beneficiaries.Dtos;
using BankingApp.Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Common.Errors;
using BankingApp.Domain.Repositories;
using BankingApp.Domain.ValueObjects;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public sealed class CreateBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const string ValidIban = "RO12BANK1234567890123456";

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();
        Mock<ISystemClock> clockMock = new();

        CreateBeneficiaryCommandHandler handler = new(
            beneficiaryRepositoryMock.Object,
            unitOfWorkMock.Object,
            clockMock.Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        CreateBeneficiaryCommand command = new(TestUserId, "John Doe", "invalid-iban", "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);

        beneficiaryRepositoryMock.VerifyAll();
        beneficiaryRepositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
        clockMock.VerifyAll();
        clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryWithSameIbanAlreadyExists_ShouldReturnDuplicateError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();
        Mock<ISystemClock> clockMock = new();

        var existingBeneficiary = Beneficiary.Create(
            TestUserId, 
            "Jane Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingBeneficiary });

        CreateBeneficiaryCommandHandler handler = new(
            beneficiaryRepositoryMock.Object,
            unitOfWorkMock.Object,
            clockMock.Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        CreateBeneficiaryCommand command = new(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.Duplicate);

        beneficiaryRepositoryMock.VerifyAll();
        beneficiaryRepositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
        clockMock.VerifyAll();
        clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIbanIsValidAndNotDuplicate_ShouldCreateAndPersistBeneficiary()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();
        Mock<ISystemClock> clockMock = new();

        DateTime now = DateTime.UtcNow;
        clockMock.Setup(clock => clock.UtcNow).Returns(now);

        beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Beneficiary>());

        beneficiaryRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.Is<Beneficiary>(b => 
                    b.UserId == TestUserId && 
                    b.Name == "John Doe" && 
                    b.Iban.Value == ValidIban &&
                    b.BankName == "Bank"), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CreateBeneficiaryCommandHandler handler = new(
            beneficiaryRepositoryMock.Object,
            unitOfWorkMock.Object,
            clockMock.Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        CreateBeneficiaryCommand command = new(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Iban.Should().Be(command.Iban);
        result.Value.BankName.Should().Be(command.BankName);
        result.Value.UserId.Should().Be(TestUserId);

        beneficiaryRepositoryMock.VerifyAll();
        beneficiaryRepositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
        clockMock.VerifyAll();
        clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenIbanIsValidAndNotDuplicate_ShouldSaveChanges()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();
        Mock<ISystemClock> clockMock = new();

        DateTime now = DateTime.UtcNow;
        clockMock.Setup(clock => clock.UtcNow).Returns(now);

        beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Beneficiary>());

        beneficiaryRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CreateBeneficiaryCommandHandler handler = new(
            beneficiaryRepositoryMock.Object,
            unitOfWorkMock.Object,
            clockMock.Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        CreateBeneficiaryCommand command = new(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        beneficiaryRepositoryMock.VerifyAll();
        beneficiaryRepositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyAll();
        unitOfWorkMock.VerifyNoOtherCalls();
        clockMock.VerifyAll();
        clockMock.VerifyNoOtherCalls();
    }
}