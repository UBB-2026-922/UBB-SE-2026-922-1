// <copyright file="RecurringPaymentProcessingServiceTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.BillPayments;
using BankingApp.Application.Services.RecurringPayments;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Tests.Services;

public class RecurringPaymentProcessingServiceTests
{
    private static readonly DateTime _fixedUtcNow = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IBillPaymentService> _billPaymentService;
    private readonly Mock<ISystemClock> _clock;
    private readonly Mock<ILogger<RecurringPaymentProcessingService>> _logger;
    private readonly Mock<IRecurringPaymentRepository> _recurringPaymentRepository;
    private readonly RecurringPaymentProcessingService _service;

    public RecurringPaymentProcessingServiceTests()
    {
        _recurringPaymentRepository = new Mock<IRecurringPaymentRepository>();
        _billPaymentService = new Mock<IBillPaymentService>();
        _clock = new Mock<ISystemClock>();
        _logger = new Mock<ILogger<RecurringPaymentProcessingService>>();

        _clock.Setup(clock => clock.UtcNow).Returns(_fixedUtcNow);

        _service = new RecurringPaymentProcessingService(
            _recurringPaymentRepository.Object,
            _billPaymentService.Object,
            _clock.Object,
            _logger.Object);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenRepositoryReturnsDuePaymentsError_ReturnsError()
    {
        // Arrange
        Error repositoryError = Error.Failure("repo.error", "DB failure");
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(repositoryError);

        // Act
        ErrorOr<Success> result = await _service.ProcessDuePaymentsAsync();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(repositoryError);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenNoDuePaymentsExist_ReturnsSuccessWithoutProcessing()
    {
        // Arrange
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment>());

        // Act
        ErrorOr<Success> result = await _service.ProcessDuePaymentsAsync();

        // Assert
        result.IsError.Should().BeFalse();
        _billPaymentService.Verify(
            billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenDuePaymentIsNotActive_SkipsPayment()
    {
        // Arrange
        RecurringPayment pausedPayment = CreatePayment(status: RecurringPaymentStatus.Paused);
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { pausedPayment });

        // Act
        ErrorOr<Success> result = await _service.ProcessDuePaymentsAsync();

        // Assert
        result.IsError.Should().BeFalse();
        _billPaymentService.Verify(
            billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenActivePaymentWithNoEndDate_AdvancesNextExecutionDate()
    {
        // Arrange
        RecurringPayment payment = CreatePayment(
            frequency: RecurringFrequency.Monthly,
            nextExecutionDate: _fixedUtcNow.AddDays(-1));

        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(new BillPayment());
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Result.Success);

        // Act
        await _service.ProcessDuePaymentsAsync();

        // Assert
        payment.NextExecutionDate.Should().Be(_fixedUtcNow.AddDays(-1).AddMonths(1));
        payment.Status.Should().Be(RecurringPaymentStatus.Active);
        _recurringPaymentRepository.Verify(repo => repo.Update(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenNextExecutionDateExceedsEndDate_CancelsPayment()
    {
        // Arrange
        DateTime nextExecution = _fixedUtcNow.AddDays(-1);
        DateTime endDate = _fixedUtcNow; // computed next (nextExecution + 1 day) == endDate, just within; use a past end date
        RecurringPayment payment = CreatePayment(
            frequency: RecurringFrequency.Monthly,
            nextExecutionDate: nextExecution,
            endDate: nextExecution.AddDays(5));  // end date before next computed run (nextExecution + 1 month)

        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(new BillPayment());
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Result.Success);

        // Act
        await _service.ProcessDuePaymentsAsync();

        // Assert
        payment.Status.Should().Be(RecurringPaymentStatus.Cancelled);
        _recurringPaymentRepository.Verify(repo => repo.Update(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenBillPaymentThrows_PausesPaymentAndPersists()
    {
        // Arrange
        RecurringPayment payment = CreatePayment();
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ThrowsAsync(new InvalidOperationException("Insufficient funds"));
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = await _service.ProcessDuePaymentsAsync();

        // Assert
        result.IsError.Should().BeFalse();
        payment.Status.Should().Be(RecurringPaymentStatus.Paused);
        _recurringPaymentRepository.Verify(repo => repo.Update(payment), Times.Once);
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenProcessingActivePayment_PassesCorrectDtoToBillPaymentService()
    {
        // Arrange
        RecurringPayment payment = CreatePayment(userId: 7, sourceAccountId: 3, billerId: 42, amount: 150m, isPayInFull: true);
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(new BillPayment());
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Result.Success);

        BillPaymentDto? capturedBillPaymentDto = null;
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .Callback<BillPaymentDto>(billPaymentDto => capturedBillPaymentDto = billPaymentDto)
            .ReturnsAsync(new BillPayment());

        // Act
        await _service.ProcessDuePaymentsAsync();

        // Assert
        capturedBillPaymentDto.Should().NotBeNull();
        capturedBillPaymentDto!.UserId.Should().Be(7);
        capturedBillPaymentDto.SourceAccountId.Should().Be(3);
        capturedBillPaymentDto.BillerId.Should().Be(42);
        capturedBillPaymentDto.Amount.Should().Be(150m);
        capturedBillPaymentDto.IsPayInFull.Should().BeTrue();
        capturedBillPaymentDto.BillerReference.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenCancellationRequestedBeforeStart_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = () => _service.ProcessDuePaymentsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ProcessDuePaymentsAsync_WhenRepositoryUpdateFailsAfterPayment_ReturnsSuccessAndLogsWarning()
    {
        // Arrange
        RecurringPayment payment = CreatePayment();
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(new BillPayment());
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Error.Failure("update.failed", "DB write failed"));

        // Act
        ErrorOr<Success> result = await _service.ProcessDuePaymentsAsync();

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Theory]
    [InlineData(RecurringFrequency.Daily, 1, 0, 0)]
    [InlineData(RecurringFrequency.Weekly, 7, 0, 0)]
    [InlineData(RecurringFrequency.BiWeekly, 14, 0, 0)]
    [InlineData(RecurringFrequency.Monthly, 0, 1, 0)]
    [InlineData(RecurringFrequency.Quarterly, 0, 3, 0)]
    [InlineData(RecurringFrequency.Yearly, 0, 0, 1)]
    public async Task ProcessDuePaymentsAsync_WhenFrequencyIsSet_AdvancesNextExecutionDateCorrectly(
        RecurringFrequency frequency,
        int expectedDays,
        int expectedMonths,
        int expectedYears)
    {
        // Arrange
        DateTime baseDate = new(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTime expectedNext = baseDate.AddDays(expectedDays).AddMonths(expectedMonths).AddYears(expectedYears);

        RecurringPayment payment = CreatePayment(frequency: frequency, nextExecutionDate: baseDate);
        _recurringPaymentRepository
            .Setup(repo => repo.GetDuePayments(_fixedUtcNow))
            .Returns(new List<RecurringPayment> { payment });
        _billPaymentService
            .Setup(billPaymentService => billPaymentService.ProcessPaymentAsync(It.IsAny<BillPaymentDto>()))
            .ReturnsAsync(new BillPayment());
        _recurringPaymentRepository
            .Setup(repo => repo.Update(payment))
            .Returns(Result.Success);

        // Act
        await _service.ProcessDuePaymentsAsync();

        // Assert
        payment.NextExecutionDate.Should().Be(expectedNext);
    }

    private static RecurringPayment CreatePayment(
        int userId = 1,
        int sourceAccountId = 10,
        int billerId = 5,
        decimal amount = 100m,
        bool isPayInFull = false,
        RecurringFrequency frequency = RecurringFrequency.Monthly,
        DateTime? nextExecutionDate = null,
        DateTime? endDate = null,
        RecurringPaymentStatus status = RecurringPaymentStatus.Active) =>
        new()
        {
            Id = 1,
            UserId = userId,
            SourceAccountId = sourceAccountId,
            BillerId = billerId,
            Amount = amount,
            IsPayInFull = isPayInFull,
            Frequency = frequency,
            NextExecutionDate = nextExecutionDate ?? _fixedUtcNow.AddDays(-1),
            EndDate = endDate,
            Status = status,
            StartDate = _fixedUtcNow.AddMonths(-1),
            CreatedAt = _fixedUtcNow.AddMonths(-1),
        };
}
