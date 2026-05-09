namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using DataAccess;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

/// <summary>
///     EF Core implementation of <see cref="IRecurringPaymentRepository" /> backed by <see cref="AppDatabaseContext" />.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="RecurringPaymentRepository" /> class.
/// </remarks>
/// <param name="context">The EF Core database context.</param>
public class RecurringPaymentRepository(AppDatabaseContext context) : IRecurringPaymentRepository
{
    /// <inheritdoc />
    public ErrorOr<RecurringPayment> Create(RecurringPayment payment)
    {
        try
        {
            if (payment.User?.Id is int userId)
            {
                User? user = context.Users.Find(userId);
                if (user is null)
                {
                    return Error.NotFound(description: $"User with id {userId} was not found.");
                }

                payment.User = user;
            }

            if (payment.Biller?.Id is int billerId)
            {
                Biller? biller = context.Billers.Find(billerId);
                if (biller is null)
                {
                    return Error.NotFound(description: $"Biller with id {billerId} was not found.");
                }

                payment.Biller = biller;
            }

            if (payment.SourceAccount?.Id is int sourceAccountId)
            {
                Account? sourceAccount = context.Accounts.Find(sourceAccountId);
                if (sourceAccount is null)
                {
                    return Error.NotFound(description: $"Account with id {sourceAccountId} was not found.");
                }

                payment.SourceAccount = sourceAccount;
            }

            context.RecurringPayments.Add(payment);
            context.SaveChanges();
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
        RecurringPayment? payment = context.RecurringPayments
            .Include(recurringPayment => recurringPayment.User)
            .Include(recurringPayment => recurringPayment.Biller)
            .Include(recurringPayment => recurringPayment.SourceAccount)
            .FirstOrDefault(recurringPayment => recurringPayment.Id == id);
        return payment ?? (ErrorOr<RecurringPayment>)Error.NotFound(description: "Recurring payment not found.");
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId)
    {
        try
        {
            return context.RecurringPayments
                .Include(recurringPayment => recurringPayment.User)
                .Include(recurringPayment => recurringPayment.Biller)
                .Include(recurringPayment => recurringPayment.SourceAccount)
                .Where(recurringPayment => EF.Property<int>(recurringPayment, "UserId") == userId)
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
            return context.RecurringPayments
                .Include(recurringPayment => recurringPayment.User)
                .Include(recurringPayment => recurringPayment.Biller)
                .Include(recurringPayment => recurringPayment.SourceAccount)
                .Where(recurringPayment => recurringPayment.Status == RecurringPaymentStatus.Active && recurringPayment.NextExecutionDate <= asOf)
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
            RecurringPayment? existingPayment = context.RecurringPayments.FirstOrDefault(recurringPayment => recurringPayment.Id == payment.Id);
            if (existingPayment is null)
            {
                return Error.NotFound(description: "Recurring payment not found.");
            }

            existingPayment.Amount = payment.Amount;
            existingPayment.IsPayInFull = payment.IsPayInFull;
            existingPayment.Frequency = payment.Frequency;
            existingPayment.StartDate = payment.StartDate;
            existingPayment.EndDate = payment.EndDate;
            existingPayment.NextExecutionDate = payment.NextExecutionDate;
            existingPayment.Status = payment.Status;
            existingPayment.CreatedAt = payment.CreatedAt;
            context.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
