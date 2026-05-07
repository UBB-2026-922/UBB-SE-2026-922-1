namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.RateAlerts;
using Utilities;
using ErrorOr;

/// <summary>
///     Implements <see cref="IRateAlertClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class RateAlertClientService : IRateAlertClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertClientService" /> class.
    /// </summary>
    public RateAlertClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public int? CurrentUserId => _apiClient.CurrentUserId;

    /// <inheritdoc />
    public Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId)
    {
        string endpoint = $"{ApiEndpoints.RateAlerts}?userId={userId}";
        return _apiClient.GetAsync<List<RateAlertDto>>(endpoint);
    }

    /// <inheritdoc />
    public Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert)
    {
        return _apiClient.PostAsync<RateAlertDto, RateAlertDto>(ApiEndpoints.RateAlerts, alert);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> DeleteAlertAsync(int alertId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.RateAlerts}/{alertId}");
    }
}
