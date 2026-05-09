namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class RateAlertProxyRepository : IRateAlertRepository
{
    private readonly IApiClient _apiClient;

    public RateAlertProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<RateAlert> GetById(int id)
        => _apiClient.GetAsync<RateAlert>($"/api/raw/rate-alerts/{id}").GetAwaiter().GetResult();

    public ErrorOr<List<RateAlert>> GetByUserId(int userId)
        => _apiClient.GetAsync<List<RateAlert>>($"/api/raw/rate-alerts/user/{userId}").GetAwaiter().GetResult();

    public ErrorOr<List<RateAlert>> GetUntriggeredAlerts()
        => _apiClient.GetAsync<List<RateAlert>>("/api/raw/rate-alerts/untriggered").GetAwaiter().GetResult();

    public ErrorOr<RateAlert> Create(RateAlert alert)
        => _apiClient.PostAsync<RateAlert, RateAlert>("/api/raw/rate-alerts", alert).GetAwaiter().GetResult();

    public ErrorOr<RateAlert> MarkTriggered(int alertId)
        => _apiClient.PutAsync<object, RateAlert>($"/api/raw/rate-alerts/{alertId}/mark-triggered", new { }).GetAwaiter().GetResult();

    public ErrorOr<Success> Delete(int id)
        => _apiClient.DeleteAsync($"/api/raw/rate-alerts/{id}").GetAwaiter().GetResult();
}
