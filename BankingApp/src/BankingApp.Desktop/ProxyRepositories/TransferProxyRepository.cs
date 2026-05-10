namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class TransferProxyRepository : ITransferRepository
{
    private readonly IApiClient _apiClient;

    public TransferProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<Transfer> GetById(int id)
        => _apiClient.GetAsync<Transfer>($"/api/transfers/{id}").GetAwaiter().GetResult();

    public ErrorOr<List<Transfer>> GetByUserId(int userId)
        => _apiClient.GetAsync<List<Transfer>>($"/api/transfers/user/{userId}").GetAwaiter().GetResult();

    public ErrorOr<Transfer> Create(Transfer transfer)
        => _apiClient.PostAsync<Transfer, Transfer>("/api/transfers", transfer).GetAwaiter().GetResult();

    public ErrorOr<Transfer> UpdateStatus(int transferId, TransferStatus status)
        => _apiClient.PutAsync<object, Transfer>(
                $"/api/transfers/{transferId}/status?status={status}",
                new { })
            .GetAwaiter()
            .GetResult();
}
