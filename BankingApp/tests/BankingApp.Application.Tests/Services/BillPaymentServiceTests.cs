// <copyright file="BillPaymentServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.BillPayments;
using BankingApp.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.Application.Tests.Services;

public class BillPaymentServiceTests
{
    private readonly Mock<IBillPaymentRepository> _repositoryMock;
    private readonly BillPaymentService _service;

    public BillPaymentServiceTests()
    {
        _repositoryMock = new Mock<IBillPaymentRepository>();
        _service = new BillPaymentService(_repositoryMock.Object);
    }

    [Theory]
    [InlineData(50, 0.50)]   // Small payment fee rule
    [InlineData(100, 0.50)]  // Boundary case
    [InlineData(150, 1.00)]  // Standard payment fee rule
    public async Task ProcessPaymentAsync_ShouldApplyCorrectFee(decimal amount, decimal expectedFee)
    {
        // Arrange
        var request = CreateValidRequest(amount);
        SetupMocks(request);

        // Act
        var result = await _service.ProcessPaymentAsync(request);

        // Assert
        result.Fee.Should().Be(expectedFee);
    }

    [Theory]
    [InlineData(999, false)]   // Below threshold
    [InlineData(1000, true)]   // Exactly threshold
    [InlineData(1500, true)]   // Above threshold
    public void Requires2FA_ShouldReturnCorrectValue(decimal amount, bool expectedResult)
    {
        // Act
        var result = _service.Requires2Fa(amount);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldPersistDataCorrectly()
    {
        // Arrange
        var request = CreateValidRequest(500);
        SetupMocks(request);

        // Act
        await _service.ProcessPaymentAsync(request);

        // Assert - Persistence check
        _repositoryMock.Verify(r => r.UpdateAccountAsync(It.IsAny<Account>()), Times.Once);
        _repositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
        _repositoryMock.Verify(r => r.AddPaymentAsync(It.IsAny<BillPayment>()), Times.Once);
    }

    private BillPaymentDto CreateValidRequest(decimal amount) => new()
    {
        UserId = 1,
        SourceAccountId = 1,
        BillerId = 1,
        BillerReference = "REF123",
        Amount = amount,
    };

    private void SetupMocks(BillPaymentDto request)
    {
        _repositoryMock.Setup(r => r.GetBillerByIdAsync(request.BillerId))
            .ReturnsAsync(new Biller { Id = request.BillerId, Name = "Test Biller" });

        _repositoryMock.Setup(r => r.GetAccountByIdAsync(request.SourceAccountId))
            .ReturnsAsync(new Account { Id = request.SourceAccountId, Balance = 5000, Currency = "RON" });
    }
}