// <copyright file="RateAlertRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertRepository class.
// </summary>

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Repositories;

/// <summary>
///     Communicates with the rate-alert API endpoints through <see cref="IApiClient"/>.
///     Provides a domain-oriented interface so ViewModels never depend on HTTP details.
/// </summary>
public class RateAlertRepository : IRateAlertRepository
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertRepository"/> class.
    /// </summary>
    /// <param name="apiClient">The HTTP API client.</param>
    public RateAlertRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId)
    {
        string endpoint = $"{ApiEndpoints.RateAlerts}?userId={userId}";
        return await _apiClient.GetAsync<List<RateAlertDto>>(endpoint);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert)
    {
        return await _apiClient.PostAsync<RateAlertDto, RateAlertDto>(ApiEndpoints.RateAlerts, alert);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> DeleteAlertAsync(int alertId)
    {
        string endpoint = $"{ApiEndpoints.RateAlerts}/{alertId}";
        return await _apiClient.DeleteAsync(endpoint);
    }
}
