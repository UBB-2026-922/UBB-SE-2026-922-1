// <copyright file="RecurringPaymentService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentService class.
// </summary>

using BankingApp.Application.DTOs.RecurringPayments;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.RecurringPayments;

/// <summary>
///     Orchestrates creation, state transitions, and retrieval of recurring payment schedules.
/// </summary>
public class RecurringPaymentService : IRecurringPaymentService
{
    private const int DailyIntervalDays = 1;
    private const int WeeklyIntervalDays = 7;
    private const int BiWeeklyIntervalDays = 14;
    private const int MonthlyIntervalMonths = 1;
    private const int QuarterlyIntervalMonths = 3;
    private const int YearlyIntervalYears = 1;

    private readonly IRecurringPaymentRepository _repository;
    private readonly ISystemClock _clock;
    private readonly ILogger<RecurringPaymentService> _logger;

    /// <summary>Initializes a new instance of the <see cref="RecurringPaymentService" /> class.</summary>
    /// <param name="repository">The recurring payment repository.</param>
    /// <param name="clock">The system clock abstraction.</param>
    /// <param name="logger">The logger instance.</param>
    public RecurringPaymentService(
        IRecurringPaymentRepository repository,
        ISystemClock clock,
        ILogger<RecurringPaymentService> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<RecurringPaymentResponse> Create(int userId, CreateRecurringPaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            return Error.Validation(
                code: "recurring_payment.invalid_amount",
                description: "Amount must be greater than zero.");
        }

        if (request.EndDate.HasValue && request.EndDate.Value <= request.StartDate)
        {
            return Error.Validation(
                code: "recurring_payment.invalid_end_date",
                description: "EndDate must be after StartDate.");
        }

        var payment = new RecurringPayment
        {
            UserId = userId,
            BillerId = request.BillerId,
            SourceAccountId = request.SourceAccountId,
            Amount = request.Amount,
            IsPayInFull = request.IsPayInFull,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextExecutionDate = ComputeNextRunDate(request.Frequency, request.StartDate),
            Status = RecurringPaymentStatus.Active,
            CreatedAt = _clock.UtcNow,
        };

        ErrorOr<RecurringPayment> createResult = _repository.Create(payment);
        if (createResult.IsError)
        {
            _logger.LogError("Failed to create recurring payment for user {UserId}: {Error}", userId, createResult.FirstError.Description);
            return createResult.FirstError;
        }

        return MapToResponse(createResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPaymentResponse>> GetByUser(int userId)
    {
        ErrorOr<List<RecurringPayment>> result = _repository.GetByUserId(userId);
        if (result.IsError)
        {
            return result.FirstError;
        }

        return result.Value.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Success> Pause(int userId, int id)
    {
        ErrorOr<RecurringPayment> findResult = _repository.GetById(id);
        if (findResult.IsError)
        {
            return findResult.FirstError;
        }

        RecurringPayment payment = findResult.Value;
        if (payment.UserId != userId)
        {
            return Error.Forbidden(description: "You do not have permission to modify this recurring payment.");
        }

        payment.Status = RecurringPaymentStatus.Paused;
        return _repository.Update(payment);
    }

    /// <inheritdoc />
    public ErrorOr<Success> Resume(int userId, int id)
    {
        ErrorOr<RecurringPayment> findResult = _repository.GetById(id);
        if (findResult.IsError)
        {
            return findResult.FirstError;
        }

        RecurringPayment payment = findResult.Value;
        if (payment.UserId != userId)
        {
            return Error.Forbidden(description: "You do not have permission to modify this recurring payment.");
        }

        if (payment.Status != RecurringPaymentStatus.Paused)
        {
            return Error.Conflict(description: "Only paused recurring payments can be resumed.");
        }

        payment.Status = RecurringPaymentStatus.Active;
        return _repository.Update(payment);
    }

    /// <inheritdoc />
    public ErrorOr<Success> Cancel(int userId, int id)
    {
        ErrorOr<RecurringPayment> findResult = _repository.GetById(id);
        if (findResult.IsError)
        {
            return findResult.FirstError;
        }

        RecurringPayment payment = findResult.Value;
        if (payment.UserId != userId)
        {
            return Error.Forbidden(description: "You do not have permission to modify this recurring payment.");
        }

        payment.Status = RecurringPaymentStatus.Cancelled;
        return _repository.Update(payment);
    }

    private static DateTime ComputeNextRunDate(RecurringFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            RecurringFrequency.Daily => from.AddDays(DailyIntervalDays),
            RecurringFrequency.Weekly => from.AddDays(WeeklyIntervalDays),
            RecurringFrequency.BiWeekly => from.AddDays(BiWeeklyIntervalDays),
            RecurringFrequency.Monthly => from.AddMonths(MonthlyIntervalMonths),
            RecurringFrequency.Quarterly => from.AddMonths(QuarterlyIntervalMonths),
            RecurringFrequency.Yearly => from.AddYears(YearlyIntervalYears),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), $"Unknown frequency: {frequency}"),
        };
    }

    private static RecurringPaymentResponse MapToResponse(RecurringPayment payment)
    {
        return new RecurringPaymentResponse
        {
            Id = payment.Id,
            UserId = payment.UserId,
            BillerId = payment.BillerId,
            SourceAccountId = payment.SourceAccountId,
            Amount = payment.Amount,
            IsPayInFull = payment.IsPayInFull,
            Frequency = payment.Frequency,
            StartDate = payment.StartDate,
            EndDate = payment.EndDate,
            NextExecutionDate = payment.NextExecutionDate,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
        };
    }
}