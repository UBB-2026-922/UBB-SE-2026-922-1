namespace BankingApp.Application.Features.ForexRateAlerts.Services;

using BankingApp.Application.Features.Forex.Services;
using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using BankingApp.Application.Features.ForexRateAlerts.Repositories;
using Domain.Aggregates.RateAlertAggregate;
using Domain.Entities;
using ErrorOr;

/// <summary>
///     Implements application-level operations for managing FX rate alerts.
/// </summary>
public class ForexRateAlertService : IForexRateAlertService
{
    private readonly IForexRateAlertRepository _rateAlertRepository;
    private readonly IForexService _exchangeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexRateAlertService" /> class.
    /// </summary>
    /// <param name="rateAlertRepository">The rate alert repository.</param>
    /// <param name="exchangeService">The exchange service used for rate lookups.</param>
    public ForexRateAlertService(IForexRateAlertRepository rateAlertRepository, IForexService exchangeService)
    {
        _rateAlertRepository = rateAlertRepository;
        _exchangeService = exchangeService;
    }

    /// <inheritdoc />
    public ErrorOr<List<ForexRateAlertDto>> GetAlerts(int userId)
    {
        ErrorOr<List<RateAlert>> result = _rateAlertRepository.GetByUserId(userId);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.ConvertAll(MapToDto);
    }

    /// <inheritdoc />
    public ErrorOr<ForexRateAlertDto> CreateAlert(ForexRateAlertDto dto)
    {
        ErrorOr<RateAlert> alertResult = RateAlert.Create(
            dto.UserId,
            dto.BaseCurrency,
            dto.TargetCurrency,
            dto.TargetRate,
            dto.IsBuyAlert,
            DateTime.UtcNow);
        if (alertResult.IsError)
        {
            return alertResult.Errors;
        }

        ErrorOr<RateAlert> createResult = _rateAlertRepository.Create(alertResult.Value);
        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        return MapToDto(createResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<Success> DeleteAlert(int id)
    {
        return _rateAlertRepository.Delete(id);
    }

    /// <inheritdoc />
    public ErrorOr<int> ProcessAlerts()
    {
        ErrorOr<List<RateAlert>> alertsResult = _rateAlertRepository.GetUntriggeredAlerts();
        if (alertsResult.IsError)
        {
            return alertsResult.Errors;
        }

        return (from alert in alertsResult.Value
            let previewResult = _exchangeService.GetRatePreview(alert.BaseCurrency, alert.TargetCurrency, 1m)
            where !previewResult.IsError
            where alert.ShouldTrigger(previewResult.Value.ExchangeRate)
            select _rateAlertRepository.MarkTriggered(alert.Id)).Count(markResult => !markResult.IsError);
    }

    private static ForexRateAlertDto MapToDto(RateAlert alert)
    {
        return new ForexRateAlertDto
        {
            Id = alert.Id,
            UserId = alert.UserId,
            BaseCurrency = alert.BaseCurrency,
            TargetCurrency = alert.TargetCurrency,
            TargetRate = alert.TargetRate,
            IsBuyAlert = alert.IsBuyAlert,
            IsTriggered = alert.IsTriggered,
            CreatedAt = alert.CreatedAt
        };
    }
}