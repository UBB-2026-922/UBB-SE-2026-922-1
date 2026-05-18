namespace BankingApp.Infrastructure.Http.Features.Billers.Services;

using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Http;
using ErrorOr;

public sealed class BillerService(IHttpClientFactory httpClientFactory) : IBillerService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken ct = default)
    {
        string endpoint = ApiEndpoints.Billers;
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

        return _http.GetErrorOrAsync<List<BillerDto>>(endpoint, ct);
    }

    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<SavedBillerDto>>(ApiEndpoints.SavedBillers, ct);

    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<SaveBillerRequest, SavedBillerDto>(ApiEndpoints.SavedBillers, request, ct);
}
