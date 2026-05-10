namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.RateAlerts;
using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Utilities;

/// <summary>
///     Implements <see cref="IRateAlertClientService" /> with desktop-side business logic and proxy repositories.
/// </summary>
internal sealed class RateAlertClientService(ICurrentSession currentSession, IRateAlertRepository rateAlertRepository)
    : IRateAlertClientService
{
    private readonly ICurrentSession _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));
    private readonly IRateAlertRepository _rateAlertRepository = rateAlertRepository ?? throw new ArgumentNullException(nameof(rateAlertRepository));

    public int? CurrentUserId => _currentSession.CurrentUserId;

    public Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId)
    {
        ErrorOr<List<RateAlert>> result = _rateAlertRepository.GetByUserId(userId);
        if (result.IsError)
        {
            return Task.FromResult<ErrorOr<List<RateAlertDto>>>(result.FirstError);
        }

        List<RateAlertDto> alerts = result.Value.ConvertAll(MapToDto);
        return Task.FromResult<ErrorOr<List<RateAlertDto>>>(alerts);
    }

    public Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert)
    {
        ErrorOr<RateAlert> alertResult = RateAlert.Create(
            alert.UserId,
            alert.BaseCurrency,
            alert.TargetCurrency,
            alert.TargetRate,
            alert.IsBuyAlert,
            DateTime.UtcNow);
        if (alertResult.IsError)
        {
            return Task.FromResult<ErrorOr<RateAlertDto>>(alertResult.FirstError);
        }

        ErrorOr<RateAlert> createResult = _rateAlertRepository.Create(alertResult.Value);
        return Task.FromResult<ErrorOr<RateAlertDto>>(createResult.IsError ? createResult.FirstError : MapToDto(createResult.Value));
    }

    public Task<ErrorOr<Success>> DeleteAlertAsync(int alertId)
        => Task.FromResult(_rateAlertRepository.Delete(alertId));

    private static RateAlertDto MapToDto(RateAlert alert)
    {
        return new RateAlertDto
        {
            Id = alert.Id,
            UserId = alert.User?.Id ?? 0,
            BaseCurrency = alert.BaseCurrency,
            TargetCurrency = alert.TargetCurrency,
            TargetRate = alert.TargetRate,
            IsBuyAlert = alert.IsBuyAlert,
            IsTriggered = alert.IsTriggered,
            CreatedAt = alert.CreatedAt,
        };
    }
}
