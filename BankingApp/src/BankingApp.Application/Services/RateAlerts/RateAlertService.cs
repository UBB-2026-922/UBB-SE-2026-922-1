// <copyright file="RateAlertService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertService class.
// </summary>

using BankingApp.Application.DTOs.Exchange;
using BankingApp.Application.DTOs.RateAlerts;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Exchange;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Services.RateAlerts;

/// <summary>
///     Implements application-level operations for managing FX rate alerts.
/// </summary>
public class RateAlertService : IRateAlertService
{
    private readonly IRateAlertRepository _rateAlertRepository;
    private readonly IExchangeService _exchangeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertService" /> class.
    /// </summary>
    /// <param name="rateAlertRepository">The rate alert repository.</param>
    /// <param name="exchangeService">The exchange service used for rate lookups.</param>
    public RateAlertService(IRateAlertRepository rateAlertRepository, IExchangeService exchangeService)
    {
        _rateAlertRepository = rateAlertRepository;
        _exchangeService = exchangeService;
    }

    /// <inheritdoc />
    public ErrorOr<List<RateAlertDto>> GetAlerts(int userId)
    {
        ErrorOr<List<RateAlert>> result = _rateAlertRepository.GetByUserId(userId);
        if (result.IsError) return result.Errors;

        return result.Value.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<RateAlertDto> CreateAlert(RateAlertDto dto)
    {
        ErrorOr<RateAlert> alertResult = RateAlert.Create(
            dto.UserId,
            dto.BaseCurrency,
            dto.TargetCurrency,
            dto.TargetRate,
            dto.IsBuyAlert,
            DateTime.UtcNow);
        if (alertResult.IsError) return alertResult.Errors;

        ErrorOr<RateAlert> createResult = _rateAlertRepository.Create(alertResult.Value);
        if (createResult.IsError) return createResult.Errors;

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
        if (alertsResult.IsError) return alertsResult.Errors;

        int triggeredCount = 0;

        foreach (RateAlert alert in alertsResult.Value)
        {
            ErrorOr<ExchangeTransactionResponseDto> previewResult = _exchangeService.GetRatePreview(
                alert.BaseCurrency,
                alert.TargetCurrency,
                1m);

            if (previewResult.IsError) continue;

            if (!alert.ShouldTrigger(previewResult.Value.ExchangeRate)) continue;

            ErrorOr<RateAlert> markResult = _rateAlertRepository.MarkTriggered(alert.Id);
            if (!markResult.IsError) triggeredCount++;
        }

        return triggeredCount;
    }

    private static RateAlertDto MapToDto(RateAlert alert)
    {
        return new RateAlertDto
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
