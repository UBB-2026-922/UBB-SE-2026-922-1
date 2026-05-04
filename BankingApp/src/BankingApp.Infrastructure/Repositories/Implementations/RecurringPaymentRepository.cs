// <copyright file="RecurringPaymentRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentRepository class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     EF Core implementation of <see cref="IRecurringPaymentRepository" /> backed by <see cref="AppDatabaseContext" />.
/// </summary>
public class RecurringPaymentRepository : IRecurringPaymentRepository
{
    private readonly AppDatabaseContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentRepository" /> class.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public RecurringPaymentRepository(AppDatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ErrorOr<RecurringPayment> Create(RecurringPayment payment)
    {
        try
        {
            _context.RecurringPayments.Add(payment);
            _context.SaveChanges();
            return payment;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<RecurringPayment> GetById(int id)
    {
        RecurringPayment? payment = _context.RecurringPayments.FirstOrDefault(r => r.Id == id);
        if (payment is null)
        {
            return Error.NotFound(description: "Recurring payment not found.");
        }

        return payment;
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId)
    {
        try
        {
            return _context.RecurringPayments
                .Where(r => r.UserId == userId)
                .ToList();
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf)
    {
        try
        {
            return _context.RecurringPayments
                .Where(r => r.Status == Domain.Enums.RecurringPaymentStatus.Active && r.NextExecutionDate <= asOf)
                .ToList();
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(RecurringPayment payment)
    {
        try
        {
            _context.RecurringPayments.Update(payment);
            _context.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
