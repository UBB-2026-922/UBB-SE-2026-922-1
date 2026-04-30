// <copyright file="BillPaymentRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy BillPaymentRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="IBillPaymentRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class BillPaymentRepository : IBillPaymentRepository
{
    /// <inheritdoc />
    public ErrorOr<BillPayment> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<BillPayment>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> GetAllBillers()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<BillPayment> Create(BillPayment payment)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<BillPayment> UpdateStatus(int paymentId, Domain.Enums.BillPaymentStatus status)
    {
        throw new NotImplementedException();
    }
}
