namespace BankingApp.Application.Features.RecurringPayments.Services;

using BankingApp.Application.Features.RecurringPayments.Repositories;
using BankingApp.Application.Common.Logging;

using BankingApp.Application.Common.Utilities;
using Domain.Entities;
using BankingApp.Application.Features.RecurringPayments.Dtos;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Orchestrates creation, state transitions, and retrieval of recurring payment schedules.
/// </summary>
public class RecurringPaymentService : IRecurringPaymentService
{
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
        ErrorOr<RecurringPayment> paymentResult = RecurringPayment.Create(
            userId,
            request.BillerId,
            request.SourceAccountId,
            request.Amount,
            request.IsPayInFull,
            request.Frequency,
            request.StartDate,
            request.EndDate,
            _clock.UtcNow);
        if (paymentResult.IsError)
        {
            return paymentResult.FirstError;
        }

        ErrorOr<RecurringPayment> createResult = _repository.Create(paymentResult.Value);
        if (!createResult.IsError)
        {
            return MapToResponse(createResult.Value);
        }

        _logger.RecurringPaymentCreateFailed(userId, createResult.FirstError.Description);
        return createResult.FirstError;

    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPaymentResponse>> GetByUser(int userId)
    {
        ErrorOr<List<RecurringPayment>> result = _repository.GetByUserId(userId);
        if (result.IsError)
        {
            return result.FirstError;
        }

        return result.Value.ConvertAll(MapToResponse);
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
        ErrorOr<Success> pauseResult = payment.Pause(userId);
        if (pauseResult.IsError)
        {
            return pauseResult.FirstError;
        }

        return _repository.Update(payment);
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResumeRecurringPayment(int userId, int id)
    {
        ErrorOr<RecurringPayment> findResult = _repository.GetById(id);
        if (findResult.IsError)
        {
            return findResult.FirstError;
        }

        RecurringPayment payment = findResult.Value;
        ErrorOr<Success> resumeResult = payment.Resume(userId);
        if (resumeResult.IsError)
        {
            return resumeResult.FirstError;
        }

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
        ErrorOr<Success> cancelResult = payment.Cancel(userId);
        if (cancelResult.IsError)
        {
            return cancelResult.FirstError;
        }

        return _repository.Update(payment);
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
            CreatedAt = payment.CreatedAt
        };
    }
}
