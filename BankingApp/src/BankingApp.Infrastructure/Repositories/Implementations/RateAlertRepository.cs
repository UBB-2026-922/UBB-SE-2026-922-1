// <copyright file="RateAlertRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy RateAlertRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="IRateAlertRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class RateAlertRepository : IRateAlertRepository
{
    /// <inheritdoc />
    public ErrorOr<RateAlert> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<RateAlert>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<RateAlert>> GetUntriggeredAlerts()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> Create(RateAlert alert)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> MarkTriggered(int alertId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int id)
    {
        throw new NotImplementedException();
    }
}
