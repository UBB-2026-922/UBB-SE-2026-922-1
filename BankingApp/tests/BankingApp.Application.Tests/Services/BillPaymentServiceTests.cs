// <copyright file="BillPaymentServiceTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
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

/// <summary>
///     Unit tests for <see cref="BillPaymentService" />.
/// </summary>
public class BillPaymentServiceTests
{
    private readonly Mock<IBillPaymentRepository> _billPaymentRepository;
    private readonly BillPaymentService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillPaymentServiceTests" /> class.
    /// </summary>
    public BillPaymentServiceTests()
    {
        _billPaymentRepository = new Mock<IBillPaymentRepository>();
        _service = new BillPaymentService(_billPaymentRepository.Object);
    }

    /// <summary>
    ///     Verifies that <see cref="BillPaymentService.ProcessPaymentAsync" /> applies the correct
    ///     fee based on the payment amount tier.
    /// </summary>
    [Theory]
    [InlineData(50, 0.50)]   // Small payment fee rule
    [InlineData(100, 0.50)]  // Boundary case
    [InlineData(150, 1.00)]  // Standard payment fee rule
    public async Task ProcessPaymentAsync_WhenAmountIsProvided_AppliesCorrectFee(decimal amount, decimal expectedFee)
    {
        // Arrange
        BillPaymentDto request = CreateValidRequest(amount);
        SetupRepositoryMocks(request);

        // Act
        BillPayment result = await _service.ProcessPaymentAsync(request);

        // Assert
        result.Fee.Should().Be(expectedFee);
    }

    /// <summary>
    ///     Verifies that <see cref="BillPaymentService.Requires2Fa" /> returns the correct value
    ///     based on whether the amount meets or exceeds the threshold.
    /// </summary>
    [Theory]
    [InlineData(999, false)]   // Below threshold
    [InlineData(1000, true)]   // Exactly threshold
    [InlineData(1500, true)]   // Above threshold
    public void Requires2Fa_WhenAmountIsChecked_ReturnsCorrectValue(decimal amount, bool expectedResult)
    {
        // Act
        bool result = _service.Requires2Fa(amount);

        // Assert
        result.Should().Be(expectedResult);
    }

    /// <summary>
    ///     Verifies that <see cref="BillPaymentService.ProcessPaymentAsync" /> persists the account
    ///     update, transaction, and payment records exactly once.
    /// </summary>
    [Fact]
    public async Task ProcessPaymentAsync_WhenPaymentIsProcessed_PersistsAllRecords()
    {
        // Arrange
        BillPaymentDto request = CreateValidRequest(500);
        SetupRepositoryMocks(request);

        // Act
        await _service.ProcessPaymentAsync(request);

        // Assert
        _billPaymentRepository.Verify(repository => repository.UpdateAccountAsync(It.IsAny<Account>()), Times.Once);
        _billPaymentRepository.Verify(repository => repository.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
        _billPaymentRepository.Verify(repository => repository.AddPaymentAsync(It.IsAny<BillPayment>()), Times.Once);
    }

    private BillPaymentDto CreateValidRequest(decimal amount) => new()
    {
        UserId = 1,
        SourceAccountId = 1,
        BillerId = 1,
        BillerReference = "REF123",
        Amount = amount,
    };

    private void SetupRepositoryMocks(BillPaymentDto request)
    {
        _billPaymentRepository.Setup(repository => repository.GetBillerByIdAsync(request.BillerId))
            .ReturnsAsync(new Biller { Id = request.BillerId, Name = "Test Biller" });

        _billPaymentRepository.Setup(repository => repository.GetAccountByIdAsync(request.SourceAccountId))
            .ReturnsAsync(new Account
            {
                Id = request.SourceAccountId,
                UserId = request.UserId,
                Balance = 5000,
                Currency = "RON",
            });
    }
}
