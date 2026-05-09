namespace BankingApp.Desktop.ProxyRepositories;

using System;
using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class BillerProxyRepository : IBillerRepository
{
    private readonly IApiClient _apiClient;

    public BillerProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<List<Biller>> GetAllBillers(bool activeOnly = true)
        => _apiClient.GetAsync<List<Biller>>($"/api/raw/billers?activeOnly={activeOnly}").GetAwaiter().GetResult();

    public ErrorOr<List<Biller>> SearchBillers(string searchTerm, string? category = null, bool activeOnly = true)
    {
        string endpoint = $"/api/raw/billers/search?searchTerm={Uri.EscapeDataString(searchTerm)}&activeOnly={activeOnly}";
        if (!string.IsNullOrWhiteSpace(category))
        {
            endpoint += $"&category={Uri.EscapeDataString(category)}";
        }

        return _apiClient.GetAsync<List<Biller>>(endpoint).GetAwaiter().GetResult();
    }

    public ErrorOr<Biller> GetBillerById(int billerId)
        => _apiClient.GetAsync<Biller>($"/api/raw/billers/{billerId}").GetAwaiter().GetResult();

    public ErrorOr<List<SavedBiller>> GetSavedBillers(int userId)
        => _apiClient.GetAsync<List<SavedBiller>>($"/api/raw/billers/saved/{userId}").GetAwaiter().GetResult();

    public ErrorOr<SavedBiller> SaveBiller(SavedBiller savedBiller)
        => _apiClient.PostAsync<SavedBiller, SavedBiller>("/api/raw/billers/saved", savedBiller).GetAwaiter().GetResult();

    public ErrorOr<Success> DeleteSavedBiller(int savedBillerId)
        => _apiClient.DeleteAsync($"/api/raw/billers/saved/{savedBillerId}").GetAwaiter().GetResult();
}
