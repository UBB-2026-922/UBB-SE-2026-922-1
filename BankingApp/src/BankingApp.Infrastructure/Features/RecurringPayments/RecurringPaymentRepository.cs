namespace BankingApp.Infrastructure.Repositories.RecurringPayments;


using BankingApp.Application.Features.RecurringPayments.Repositories;
using Domain.Entities;
using Domain.Enums;
using BankingApp.Infrastructure.DataAccess;
using ErrorOr;
using Persistence;

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
        RecurringPayment? payment = context.RecurringPayments.FirstOrDefault(recurringPayment => recurringPayment.Id == id);
        return payment ?? (ErrorOr<RecurringPayment>)Error.NotFound(description: "Recurring payment not found.");
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId)
    {
        try
        {
            return context.RecurringPayments
                .Where(recurringPayment => recurringPayment.UserId == userId)
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
            context.RecurringPayments.Update(payment);
            context.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
