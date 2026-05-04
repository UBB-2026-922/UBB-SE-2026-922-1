// <copyright file="RecurringPaymentRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy RecurringPaymentRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="IRecurringPaymentRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class RecurringPaymentRepository : IRecurringPaymentRepository
{
    /// <inheritdoc />
    public ErrorOr<RecurringPayment> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<RecurringPayment> Create(RecurringPayment payment)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<RecurringPayment> Update(RecurringPayment payment)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Success> Cancel(int id)
    {
        throw new NotImplementedException();
    }
}
