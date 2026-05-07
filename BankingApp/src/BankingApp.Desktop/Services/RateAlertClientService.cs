using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.RateAlerts;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class RateAlertClientService : IRateAlertClientService
{
    private readonly IApiClient _apiClient;

    public RateAlertClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public int? CurrentUserId => _apiClient.CurrentUserId;

    public Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId)
    {
        string endpoint = $"{ApiEndpoints.RateAlerts}?userId={userId}";
        return _apiClient.GetAsync<List<RateAlertDto>>(endpoint);
    }

    public Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert)
    {
        return _apiClient.PostAsync<RateAlertDto, RateAlertDto>(ApiEndpoints.RateAlerts, alert);
    }

    public Task<ErrorOr<Success>> DeleteAlertAsync(int alertId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.RateAlerts}/{alertId}");
    }
}
