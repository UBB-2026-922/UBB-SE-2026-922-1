namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.BillPayments;
using Application.DTOs.Billers;
using Application.DTOs.RecurringPayments;
using Utilities;
using ErrorOr;

/// <summary>
///     Implements <see cref="IBillPaymentClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class BillPaymentClientService : IBillPaymentClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillPaymentClientService" /> class.
    /// </summary>
    public BillPaymentClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
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
        {
            endpoint += $"{separator}category={Uri.EscapeDataString(category)}";
        }

        return _apiClient.GetAsync<List<BillerDto>>(endpoint);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync()
    {
        return _apiClient.GetAsync<List<SavedBillerDto>>(ApiEndpoints.BillPaySavedBillers);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<AccountDto>>> GetAccountsAsync()
    {
        return _apiClient.GetAsync<List<AccountDto>>(ApiEndpoints.BillPayAccounts);
    }

    /// <inheritdoc />
    public Task<ErrorOr<FeeResponse>> GetFeeAsync(decimal amount)
    {
        return _apiClient.GetAsync<FeeResponse>($"{ApiEndpoints.BillPayFee}?amount={amount}");
    }

    /// <inheritdoc />
    public Task<ErrorOr<RequiresTwoFaResponse>> GetRequires2FaAsync(decimal amount)
    {
        return _apiClient.GetAsync<RequiresTwoFaResponse>($"{ApiEndpoints.BillPayRequires2Fa}?amount={amount}");
    }

    /// <inheritdoc />
    public Task<ErrorOr<BillPayResponse>> PayBillAsync(BillPayRequest request)
    {
        return _apiClient.PostAsync<BillPayRequest, BillPayResponse>(ApiEndpoints.BillPayPay, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request)
    {
        return _apiClient.PostAsync<SaveBillerRequest, SavedBillerDto>(ApiEndpoints.BillPaySaveBiller, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetRecurringPaymentsAsync()
    {
        return _apiClient.GetAsync<List<RecurringPaymentResponse>>(ApiEndpoints.RecurringPayments);
    }

    /// <inheritdoc />
    public Task<ErrorOr<RecurringPaymentResponse>> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request)
    {
        return _apiClient.PostAsync<CreateRecurringPaymentRequest, RecurringPaymentResponse>(
            ApiEndpoints.RecurringPayments, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> PauseRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.PutAsync<object>($"{ApiEndpoints.RecurringPayments}/{paymentId}/pause", new { });
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> ResumeRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.PutAsync<object>($"{ApiEndpoints.RecurringPayments}/{paymentId}/resume", new { });
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> CancelRecurringPaymentAsync(int paymentId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.RecurringPayments}/{paymentId}");
    }
}
