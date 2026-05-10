namespace BankingApp.Desktop.ProxyRepositories;

using System;
using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class RecurringPaymentProxyRepository : IRecurringPaymentRepository
{
    private readonly IApiClient _apiClient;

    public RecurringPaymentProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<RecurringPayment> GetById(int id)
        => _apiClient.GetAsync<RecurringPayment>($"/api/recurring-payments/{id}").GetAwaiter().GetResult();

    public ErrorOr<List<RecurringPayment>> GetByUserId(int userId)
        => _apiClient.GetAsync<List<RecurringPayment>>($"/api/recurring-payments/user/{userId}").GetAwaiter().GetResult();

    public ErrorOr<List<RecurringPayment>> GetDuePayments(DateTime asOf)
        => _apiClient.GetAsync<List<RecurringPayment>>($"/api/recurring-payments/due?asOf={Uri.EscapeDataString(asOf.ToString("O"))}").GetAwaiter().GetResult();

    public ErrorOr<RecurringPayment> Create(RecurringPayment payment)
        => _apiClient.PostAsync<RecurringPayment, RecurringPayment>("/api/recurring-payments", payment).GetAwaiter().GetResult();

    public ErrorOr<Success> Update(RecurringPayment payment)
        => _apiClient.PutAsync("/api/recurring-payments", payment).GetAwaiter().GetResult();
}
