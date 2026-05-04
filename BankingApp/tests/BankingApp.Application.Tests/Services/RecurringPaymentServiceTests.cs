// <copyright file="RecurringPaymentServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.DTOs.RecurringPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.RecurringPayments;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="RecurringPaymentService" />.
/// </summary>
public class RecurringPaymentServiceTests
{
    private static readonly DateTime FrozenUtcNow = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FixedStartDate = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRecurringPaymentRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clock = new(MockBehavior.Strict);
    private readonly RecurringPaymentService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentServiceTests" /> class.
    /// </summary>
    public RecurringPaymentServiceTests()
    {
        _service = new RecurringPaymentService(
            _repository.Object,
            _clock.Object,
            NullLogger<RecurringPaymentService>.Instance);
    }

    // -----------------------------------------------------------------------
    // Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WhenAmountIsZero_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 1,
            SourceAccountId = 2,
            Amount = 0m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 99, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("recurring_payment.invalid_amount");
    }

    [Fact]
    public void Create_WhenAmountIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 1,
            SourceAccountId = 2,
            Amount = -50m,
            Frequency = RecurringFrequency.Weekly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 99, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WhenEndDateIsBeforeStartDate_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 1,
            SourceAccountId = 2,
            Amount = 100m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = FixedStartDate,
            EndDate = FixedStartDate.AddDays(-1),
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 99, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("recurring_payment.invalid_end_date");
    }

    [Fact]
    public void Create_WhenFrequencyIsMonthly_ShouldSetNextExecutionDateOneMonthAfterStart()
    {
        // Arrange
        _clock.Setup(c => c.UtcNow).Returns(FrozenUtcNow);
        var savedPayment = new RecurringPayment
        {
            Id = 1,
            UserId = 42,
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 200m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = FixedStartDate,
            NextExecutionDate = FixedStartDate.AddMonths(1),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = FrozenUtcNow,
        };
        _repository
            .Setup(r => r.Create(It.IsAny<RecurringPayment>()))
            .Returns((ErrorOr<RecurringPayment>)savedPayment);

        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 200m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 42, request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.NextExecutionDate.Should().Be(FixedStartDate.AddMonths(1));
    }

    [Fact]
    public void Create_WhenFrequencyIsWeekly_ShouldSetNextExecutionDateSevenDaysAfterStart()
    {
        // Arrange
        _clock.Setup(c => c.UtcNow).Returns(FrozenUtcNow);
        var savedPayment = new RecurringPayment
        {
            Id = 2,
            UserId = 42,
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 50m,
            Frequency = RecurringFrequency.Weekly,
            StartDate = FixedStartDate,
            NextExecutionDate = FixedStartDate.AddDays(7),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = FrozenUtcNow,
        };
        _repository
            .Setup(r => r.Create(It.IsAny<RecurringPayment>()))
            .Returns((ErrorOr<RecurringPayment>)savedPayment);

        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 50m,
            Frequency = RecurringFrequency.Weekly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 42, request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.NextExecutionDate.Should().Be(FixedStartDate.AddDays(7));
    }

    [Fact]
    public void Create_WhenFrequencyIsYearly_ShouldSetNextExecutionDateOneYearAfterStart()
    {
        // Arrange
        _clock.Setup(c => c.UtcNow).Returns(FrozenUtcNow);
        var savedPayment = new RecurringPayment
        {
            Id = 3,
            UserId = 42,
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 1200m,
            Frequency = RecurringFrequency.Yearly,
            StartDate = FixedStartDate,
            NextExecutionDate = FixedStartDate.AddYears(1),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = FrozenUtcNow,
        };
        _repository
            .Setup(r => r.Create(It.IsAny<RecurringPayment>()))
            .Returns((ErrorOr<RecurringPayment>)savedPayment);

        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 7,
            SourceAccountId = 3,
            Amount = 1200m,
            Frequency = RecurringFrequency.Yearly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 42, request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.NextExecutionDate.Should().Be(FixedStartDate.AddYears(1));
    }

    [Fact]
    public void Create_WhenRequestIsValid_ShouldStampCreatedAtWithFrozenClock()
    {
        // Arrange
        _clock.Setup(c => c.UtcNow).Returns(FrozenUtcNow);
        var savedPayment = new RecurringPayment
        {
            Id = 10,
            UserId = 5,
            BillerId = 1,
            SourceAccountId = 1,
            Amount = 75m,
            Frequency = RecurringFrequency.Daily,
            StartDate = FixedStartDate,
            NextExecutionDate = FixedStartDate.AddDays(1),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = FrozenUtcNow,
        };
        _repository
            .Setup(r => r.Create(It.IsAny<RecurringPayment>()))
            .Returns((ErrorOr<RecurringPayment>)savedPayment);

        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 1,
            SourceAccountId = 1,
            Amount = 75m,
            Frequency = RecurringFrequency.Daily,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 5, request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CreatedAt.Should().Be(FrozenUtcNow);
    }

    [Fact]
    public void Create_WhenRepositoryFails_ShouldPropagateFailureError()
    {
        // Arrange
        _clock.Setup(c => c.UtcNow).Returns(FrozenUtcNow);
        _repository
            .Setup(r => r.Create(It.IsAny<RecurringPayment>()))
            .Returns(Error.Failure(description: "DB connection lost."));

        var request = new CreateRecurringPaymentRequest
        {
            BillerId = 1,
            SourceAccountId = 1,
            Amount = 100m,
            Frequency = RecurringFrequency.Monthly,
            StartDate = FixedStartDate,
        };

        // Act
        ErrorOr<RecurringPaymentResponse> result = _service.Create(userId: 5, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Description.Should().Be("DB connection lost.");
    }

    // -----------------------------------------------------------------------
    // GetByUser
    // -----------------------------------------------------------------------

    [Fact]
    public void GetByUser_WhenUserHasPayments_ShouldReturnMappedList()
    {
        // Arrange
        var payments = new List<RecurringPayment>
        {
            new() { Id = 1, UserId = 10, Amount = 50m, Frequency = RecurringFrequency.Monthly, Status = RecurringPaymentStatus.Active },
            new() { Id = 2, UserId = 10, Amount = 30m, Frequency = RecurringFrequency.Weekly,  Status = RecurringPaymentStatus.Paused },
        };
        _repository.Setup(r => r.GetByUserId(10)).Returns((ErrorOr<List<RecurringPayment>>)payments);

        // Act
        ErrorOr<List<RecurringPaymentResponse>> result = _service.GetByUser(userId: 10);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void GetByUser_WhenRepositoryFails_ShouldReturnFailureError()
    {
        // Arrange
        _repository.Setup(r => r.GetByUserId(99)).Returns(Error.Failure(description: "Query timed out."));

        // Act
        ErrorOr<List<RecurringPaymentResponse>> result = _service.GetByUser(userId: 99);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    // -----------------------------------------------------------------------
    // Pause
    // -----------------------------------------------------------------------

    [Fact]
    public void Pause_WhenPaymentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _repository.Setup(r => r.GetById(999)).Returns(Error.NotFound(description: "Recurring payment not found."));

        // Act
        ErrorOr<Success> result = _service.Pause(userId: 1, id: 999);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Pause_WhenUserDoesNotOwnPayment_ShouldReturnForbiddenError()
    {
        // Arrange
        var payment = new RecurringPayment { Id = 5, UserId = 10, Status = RecurringPaymentStatus.Active };
        _repository.Setup(r => r.GetById(5)).Returns((ErrorOr<RecurringPayment>)payment);

        // Act
        ErrorOr<Success> result = _service.Pause(userId: 99, id: 5);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public void Pause_WhenPaymentIsOwnedByUser_ShouldSetStatusToPausedAndReturnSuccess()
    {
        // Arrange
        var payment = new RecurringPayment { Id = 5, UserId = 42, Status = RecurringPaymentStatus.Active };
        _repository.Setup(r => r.GetById(5)).Returns((ErrorOr<RecurringPayment>)payment);
        _repository.Setup(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Paused)))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Pause(userId: 42, id: 5);

        // Assert
        result.IsError.Should().BeFalse();
        _repository.Verify(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Paused)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Resume
    // -----------------------------------------------------------------------

    [Fact]
    public void Resume_WhenPaymentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _repository.Setup(r => r.GetById(888)).Returns(Error.NotFound(description: "Recurring payment not found."));

        // Act
        ErrorOr<Success> result = _service.Resume(userId: 1, id: 888);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Resume_WhenPaymentIsPaused_ShouldSetStatusToActiveAndReturnSuccess()
    {
        // Arrange
        var payment = new RecurringPayment { Id = 6, UserId = 42, Status = RecurringPaymentStatus.Paused };
        _repository.Setup(r => r.GetById(6)).Returns((ErrorOr<RecurringPayment>)payment);
        _repository.Setup(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Active)))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Resume(userId: 42, id: 6);

        // Assert
        result.IsError.Should().BeFalse();
        _repository.Verify(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Active)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Cancel
    // -----------------------------------------------------------------------

    [Fact]
    public void Cancel_WhenPaymentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        _repository.Setup(r => r.GetById(777)).Returns(Error.NotFound(description: "Recurring payment not found."));

        // Act
        ErrorOr<Success> result = _service.Cancel(userId: 1, id: 777);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Cancel_WhenUserDoesNotOwnPayment_ShouldReturnForbiddenError()
    {
        // Arrange
        var payment = new RecurringPayment { Id = 7, UserId = 10, Status = RecurringPaymentStatus.Active };
        _repository.Setup(r => r.GetById(7)).Returns((ErrorOr<RecurringPayment>)payment);

        // Act
        ErrorOr<Success> result = _service.Cancel(userId: 55, id: 7);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public void Cancel_WhenPaymentIsOwnedByUser_ShouldSetStatusToCancelledAndReturnSuccess()
    {
        // Arrange
        var payment = new RecurringPayment { Id = 7, UserId = 42, Status = RecurringPaymentStatus.Active };
        _repository.Setup(r => r.GetById(7)).Returns((ErrorOr<RecurringPayment>)payment);
        _repository.Setup(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Cancelled)))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Cancel(userId: 42, id: 7);

        // Assert
        result.IsError.Should().BeFalse();
        _repository.Verify(r => r.Update(It.Is<RecurringPayment>(p => p.Status == RecurringPaymentStatus.Cancelled)), Times.Once);
    }
}
