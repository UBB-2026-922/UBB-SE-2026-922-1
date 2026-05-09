namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class ExchangeProxyRepository : IExchangeRepository
{
    private readonly IApiClient _apiClient;

    public ExchangeProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<ExchangeTransaction> GetById(int id)
        => _apiClient.GetAsync<ExchangeTransaction>($"/api/raw/exchanges/{id}").GetAwaiter().GetResult();

    public ErrorOr<List<ExchangeTransaction>> GetByUserId(int userId)
        => _apiClient.GetAsync<List<ExchangeTransaction>>($"/api/raw/exchanges/user/{userId}").GetAwaiter().GetResult();

    public ErrorOr<ExchangeTransaction> Create(ExchangeTransaction exchange)
        => _apiClient.PostAsync<ExchangeTransaction, ExchangeTransaction>("/api/raw/exchanges", exchange).GetAwaiter().GetResult();

    public ErrorOr<ExchangeTransaction> UpdateStatus(int exchangeId, ExchangeTransactionStatus status)
        => _apiClient.PutAsync<object, ExchangeTransaction>($"/api/raw/exchanges/{exchangeId}/status", new UpdateStatusRequest { Status = status })
            .GetAwaiter()
            .GetResult();

    private sealed class UpdateStatusRequest
    {
        public ExchangeTransactionStatus Status { get; set; }
    }
}
