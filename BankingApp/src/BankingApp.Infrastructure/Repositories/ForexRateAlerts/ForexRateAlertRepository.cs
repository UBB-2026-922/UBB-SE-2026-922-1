namespace BankingApp.Infrastructure.Repositories.ForexRateAlerts;


using BankingApp.Application.Features.ForexRateAlerts.Repositories;
using Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using ErrorOr;

/// <summary>
///     EF Core implementation of <see cref="IForexRateAlertRepository" />.
/// </summary>
public class ForexRateAlertRepository : IForexRateAlertRepository
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexRateAlertRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The application database context.</param>
    public ForexRateAlertRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<RateAlert> GetById(int id)
    {
        RateAlert? alert = _databaseContext.RateAlerts.FirstOrDefault(rateAlert => rateAlert.Id == id);
        if (alert is null)
        {
            return Error.NotFound(description: "Rate alert not found.");
        }

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
            if (alert is null)
            {
                return Error.NotFound(description: "Rate alert not found.");
            }

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
            if (alert is null)
            {
                return Error.NotFound(description: "Rate alert not found.");
            }

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
