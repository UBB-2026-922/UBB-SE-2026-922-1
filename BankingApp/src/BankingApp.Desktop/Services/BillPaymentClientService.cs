using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.BillPayments;
using BankingApp.Application.DTOs.Billers;
using BankingApp.Application.DTOs.RecurringPayments;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class BillPaymentClientService : IBillPaymentClientService
{
    private readonly IApiClient _apiClient;

    public BillPaymentClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null)
    {
        string endpoint = ApiEndpoints.BillPayBillers;
        string separator = "?";
        if (!string.IsNullOrWhiteSpace(search))
        {
            endpoint += $"{separator}search={Uri.EscapeDataString(search)}";
            separator = "&";
        }

        if (!string.IsNullOrWhiteSpace(category))
            endpoint += $"{separator}category={Uri.EscapeDataString(category)}";

        return _apiClient.GetAsync<List<BillerDto>>(endpoint);
    }

    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync()
    {
        return _apiClient.GetAsync<List<SavedBillerDto>>(ApiEndpoints.BillPaySavedBillers);
    }

    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync()
    {
        return _apiClient.GetAsync<List<AccountDto>>(ApiEndpoints.BillPayAccounts);
    }

    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount)
    {
        return _apiClient.GetAsync<FeeResponse>($"{ApiEndpoints.BillPayFee}?amount={amount}");
    }

    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount)
    {
        return _apiClient.GetAsync<RequiresTwoFaResponse>(
            $"{ApiEndpoints.BillPayRequires2Fa}?amount={amount}");
    }

    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request)
    {
        return _apiClient.PostAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayPay, request);
    }

    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request)
    {
        return _apiClient.PostAsync<SaveBillerRequest, SavedBillerDto>(ApiEndpoints.BillPaySaveBiller, request);
    }

    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetRecurringPaymentsAsync()
    {
        return _apiClient.GetAsync<List<RecurringPaymentResponse>>(ApiEndpoints.RecurringPayments);
    }

    public Task<ErrorOr<RecurringPaymentResponse>> CreateRecurringPaymentAsync(
        CreateRecurringPaymentRequest request)
    {
        return _apiClient.PostAsync<CreateRecurringPaymentRequest, RecurringPaymentResponse>(
            ApiEndpoints.RecurringPayments, request);
    }

    public Task<ErrorOr<Success>> PauseRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.PutAsync<object>(
            $"{ApiEndpoints.RecurringPayments}/{paymentId}/pause",
            new { });
    }

    public Task<ErrorOr<Success>> ResumeRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.PutAsync<object>(
            $"{ApiEndpoints.RecurringPayments}/{paymentId}/resume",
            new { });
    }

    public Task<ErrorOr<Success>> CancelRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.RecurringPayments}/{paymentId}");
    }
}
