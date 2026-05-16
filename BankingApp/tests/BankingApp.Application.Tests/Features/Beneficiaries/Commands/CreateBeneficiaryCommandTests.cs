namespace BankingApp.Application.Tests.Features.Beneficiaries.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Common.Contracts; // Adăugat pentru IUnitOfWork
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

using MockFactory = BankingApp.Application.Tests.MockFactory;

public sealed class CreateBeneficiaryCommandTests
{
    private const int TestUserId = 1;
    private const string ValidIban = "RO12BANK1234567890123456";

    [Fact]
    public async Task Handle_WhenIbanIsInvalid_ShouldReturnInvalidIbanError()
    {
        // Arrange
        var handler = new CreateBeneficiaryCommandHandler(
            MockFactory.CreateBeneficiaryRepositoryMock().Object,
            MockFactory.CreateUnitOfWorkMock().Object,
            MockFactory.CreateSystemClockMock().Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        var command = new CreateBeneficiaryCommand(TestUserId, "John Doe", "invalid-iban", "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransferErrors.InvalidIban);
    }

    [Fact]
    public async Task Handle_WhenBeneficiaryWithSameIbanAlreadyExists_ShouldReturnDuplicateError()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        var existingBeneficiary = Beneficiary.Create(
            TestUserId, 
            "Jane Doe", 
            Iban.Create(ValidIban).Value, 
            null, 
            DateTime.UtcNow);

        beneficiaryRepositoryMock
            .Setup(repository => repository.ListByUserIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingBeneficiary });

        var handler = new CreateBeneficiaryCommandHandler(
            beneficiaryRepositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object,
            MockFactory.CreateSystemClockMock().Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        var command = new CreateBeneficiaryCommand(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BeneficiaryErrors.Duplicate);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateAndPersistBeneficiary()
    {
        // Arrange
        Mock<IBeneficiaryRepository> beneficiaryRepositoryMock = MockFactory.CreateBeneficiaryRepositoryMock();
        
        var handler = new CreateBeneficiaryCommandHandler(
            beneficiaryRepositoryMock.Object,
            MockFactory.CreateUnitOfWorkMock().Object,
            MockFactory.CreateSystemClockMock().Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        var command = new CreateBeneficiaryCommand(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Iban.Should().Be(command.Iban);
        result.Value.BankName.Should().Be(command.BankName);
        result.Value.UserId.Should().Be(TestUserId);

        beneficiaryRepositoryMock.Verify(repository => repository.AddAsync(
            It.Is<Beneficiary>(b => 
                b.UserId == command.UserId && 
                b.Name == command.Name && 
                b.Iban.Value == command.Iban &&
                b.BankName == command.BankName), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        Mock<IUnitOfWork> unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        
        var handler = new CreateBeneficiaryCommandHandler(
            MockFactory.CreateBeneficiaryRepositoryMock().Object,
            unitOfWorkMock.Object,
            MockFactory.CreateSystemClockMock().Object,
            NullLogger<CreateBeneficiaryCommandHandler>.Instance);

        var command = new CreateBeneficiaryCommand(TestUserId, "John Doe", ValidIban, "Bank");

        // Act
        ErrorOr<BeneficiaryDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}