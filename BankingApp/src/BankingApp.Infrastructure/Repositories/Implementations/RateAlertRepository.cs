// <copyright file="RateAlertRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertRepository class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     EF Core implementation of <see cref="IRateAlertRepository" />.
/// </summary>
public class RateAlertRepository : IRateAlertRepository
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The application database context.</param>
    public RateAlertRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> GetById(int id)
    {
        RateAlert? alert = _databaseContext.RateAlerts.FirstOrDefault(rateAlert => rateAlert.Id == id);
        if (alert is null) return Error.NotFound(description: "Rate alert not found.");

        return alert;
    }

    /// <inheritdoc />
    public ErrorOr<List<RateAlert>> GetByUserId(int userId)
    {
        try
        {
            return _databaseContext.RateAlerts
                .Where(alert => alert.UserId == userId)
                .OrderByDescending(alert => alert.CreatedAt)
                .ToList();
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<List<RateAlert>> GetUntriggeredAlerts()
    {
        try
        {
            return _databaseContext.RateAlerts
                .Where(alert => !alert.IsTriggered)
                .OrderBy(alert => alert.CreatedAt)
                .ToList();
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> Create(RateAlert alert)
    {
        try
        {
            _databaseContext.RateAlerts.Add(alert);
            _databaseContext.SaveChanges();
            return alert;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> MarkTriggered(int alertId)
    {
        try
        {
            RateAlert? alert = _databaseContext.RateAlerts.FirstOrDefault(rateAlert => rateAlert.Id == alertId);
            if (alert is null) return Error.NotFound(description: "Rate alert not found.");

            alert.IsTriggered = true;
            _databaseContext.SaveChanges();
            return alert;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int id)
    {
        try
        {
            RateAlert? alert = _databaseContext.RateAlerts.FirstOrDefault(rateAlert => rateAlert.Id == id);
            if (alert is null) return Error.NotFound(description: "Rate alert not found.");

            _databaseContext.RateAlerts.Remove(alert);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }
}