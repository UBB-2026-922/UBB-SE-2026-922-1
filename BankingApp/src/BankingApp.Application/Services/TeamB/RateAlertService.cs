// <copyright file="RateAlertService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertService class.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Implements application-level operations for managing FX rate alerts.
/// </summary>
public class RateAlertService : IRateAlertService
{
    private const int RatePrecisionDecimals = 2;

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
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<RateAlertDto> CreateAlert(RateAlertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BaseCurrency))
        {
            return Error.Validation(description: "Base currency cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(dto.TargetCurrency))
        {
            return Error.Validation(description: "Target currency cannot be empty.");
        }

        if (dto.BaseCurrency.Equals(dto.TargetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(description: "Base and target currencies must differ.");
        }

        if (dto.TargetRate <= 0)
        {
            return Error.Validation(description: "Target rate must be greater than zero.");
        }

        RateAlert alert = new RateAlert
        {
            UserId = dto.UserId,
            BaseCurrency = dto.BaseCurrency,
            TargetCurrency = dto.TargetCurrency,
            TargetRate = dto.TargetRate,
            IsBuyAlert = dto.IsBuyAlert,
            IsTriggered = false,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<RateAlert> createResult = _rateAlertRepository.Create(alert);
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

        int triggeredCount = 0;

        foreach (RateAlert alert in alertsResult.Value)
        {
            ErrorOr<ExchangeTransactionResponseDto> previewResult = _exchangeService.GetRatePreview(
                alert.BaseCurrency,
                alert.TargetCurrency,
                1m);

            if (previewResult.IsError)
            {
                continue;
            }

            decimal currentRate = Math.Round(previewResult.Value.ExchangeRate, RatePrecisionDecimals);
            decimal targetRate = Math.Round(alert.TargetRate, RatePrecisionDecimals);

            bool shouldTrigger = alert.IsBuyAlert
                ? currentRate <= targetRate
                : currentRate >= targetRate;

            if (!shouldTrigger)
            {
                continue;
            }

            ErrorOr<RateAlert> markResult = _rateAlertRepository.MarkTriggered(alert.Id);
            if (!markResult.IsError)
            {
                triggeredCount++;
            }
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
            CreatedAt = alert.CreatedAt,
        };
    }
}