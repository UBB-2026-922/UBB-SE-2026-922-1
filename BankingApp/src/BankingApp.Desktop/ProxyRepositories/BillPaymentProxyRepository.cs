namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class BillPaymentProxyRepository : IBillPaymentRepository
{
    private readonly IApiClient _apiClient;

    public BillPaymentProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Biller>> GetBillersAsync()
    {
        ErrorOr<List<Biller>> result = await _apiClient.GetAsync<List<Biller>>("/api/raw/bill-payments/billers");
        return result.IsError ? [] : result.Value;
    }

    public async Task<Biller?> GetBillerByIdAsync(int billerId)
    {
        ErrorOr<Biller> result = await _apiClient.GetAsync<Biller>($"/api/raw/bill-payments/billers/{billerId}");
        return result.IsError ? null : result.Value;
    }

    public async Task AddPaymentAsync(BillPayment payment)
        => _ = await _apiClient.PostAsync<BillPayment, BillPayment>("/api/raw/bill-payments/payments", payment);

    public async Task<IEnumerable<BillPayment>> GetUserPaymentHistoryAsync(int userId)
    {
        ErrorOr<List<BillPayment>> result = await _apiClient.GetAsync<List<BillPayment>>($"/api/raw/bill-payments/payments/user/{userId}");
        return result.IsError ? [] : result.Value;
    }

    public async Task<IEnumerable<SavedBiller>> GetSavedBillersAsync(int userId)
    {
        ErrorOr<List<SavedBiller>> result = await _apiClient.GetAsync<List<SavedBiller>>($"/api/raw/bill-payments/saved-billers/{userId}");
        return result.IsError ? [] : result.Value;
    }

    public async Task AddSavedBillerAsync(SavedBiller savedBiller)
        => _ = await _apiClient.PostAsync<SavedBiller, SavedBiller>("/api/raw/bill-payments/saved-billers", savedBiller);

    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        ErrorOr<Account> result = await _apiClient.GetAsync<Account>($"/api/raw/bill-payments/accounts/{accountId}");
        return result.IsError ? null : result.Value;
    }

    public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(int userId)
    {
        ErrorOr<List<Account>> result = await _apiClient.GetAsync<List<Account>>($"/api/raw/bill-payments/accounts/user/{userId}");
        return result.IsError ? [] : result.Value;
    }

    public async Task UpdateAccountAsync(Account account)
        => _ = await _apiClient.PutAsync("/api/raw/bill-payments/accounts", account);

    public async Task AddTransactionAsync(Transaction transaction)
        => _ = await _apiClient.PostAsync<Transaction, Transaction>("/api/raw/bill-payments/transactions", transaction);
}
