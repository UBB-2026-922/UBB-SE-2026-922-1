namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

/// <summary>
///     Desktop-side proxy for <see cref="IDashboardRepository" /> that forwards repository calls over HTTP.
/// </summary>
internal sealed class DashboardProxyRepository : IDashboardRepository
{
    private readonly IApiClient _apiClient;

    public DashboardProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<List<Account>> GetAccountsByUser(int userId)
        => _apiClient.GetAsync<List<Account>>($"/api/dashboard/raw/accounts/{userId}").GetAwaiter().GetResult();

    public ErrorOr<List<Card>> GetCardsByUser(int userId)
        => _apiClient.GetAsync<List<Card>>($"/api/dashboard/raw/cards/{userId}").GetAwaiter().GetResult();

    public ErrorOr<List<Transaction>> GetRecentTransactions(int accountId, int limit = IDashboardRepository.DefaultRecentTransactionLimit)
        => _apiClient.GetAsync<List<Transaction>>($"/api/dashboard/raw/transactions/{accountId}?limit={limit}").GetAwaiter().GetResult();

    public ErrorOr<int> GetUnreadNotificationCount(int userId)
        => _apiClient.GetAsync<int>($"/api/dashboard/raw/notifications/{userId}/unread-count").GetAwaiter().GetResult();

    public ErrorOr<Transfer> AddTransfer(Transfer transfer)
        => _apiClient.PostAsync<Transfer, Transfer>("/api/dashboard/raw/transfers", transfer).GetAwaiter().GetResult();

    public ErrorOr<List<Transfer>> GetTransfersByUserId(int userId)
        => _apiClient.GetAsync<List<Transfer>>($"/api/dashboard/raw/transfers/{userId}").GetAwaiter().GetResult();

    public ErrorOr<Success> DebitAccount(int accountId, decimal amount)
        => _apiClient.PostAsync("/api/dashboard/raw/accounts/" + accountId + "/debit", new DebitAccountRequest { Amount = amount })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Transaction> AddTransaction(Transaction transaction)
        => _apiClient.PostAsync<Transaction, Transaction>("/api/dashboard/raw/transactions", transaction).GetAwaiter().GetResult();

    private sealed class DebitAccountRequest
    {
        public decimal Amount { get; set; }
    }
}
