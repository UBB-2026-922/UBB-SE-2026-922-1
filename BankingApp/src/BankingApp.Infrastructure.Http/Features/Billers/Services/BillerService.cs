namespace BankingApp.Infrastructure.Http.Features.Billers.Services;

using Contracts.Features.Billers.Dtos;
using Contracts.Features.Billers.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class BillerService(IHttpClientFactory httpClientFactory, ILogger<BillerService> logger) : IBillerService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<BillerService> _logger = logger;

    public Task<ErrorOr<List<BillerDto>>> GetBillersAsync(string? search = null, string? category = null, CancellationToken ct = default)
    {
        string endpoint = ApiEndpoints.Billers.Base;
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

        return _http.GetErrorOrAsync<List<BillerDto>>(endpoint, _logger, ct);
    }

    public Task<ErrorOr<List<SavedBillerDto>>> GetSavedBillersAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<SavedBillerDto>>(ApiEndpoints.Billers.SavedFull, _logger, ct);

    public Task<ErrorOr<SavedBillerDto>> SaveBillerAsync(SaveBillerRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<SaveBillerRequest, SavedBillerDto>(ApiEndpoints.Billers.SavedFull, request, _logger, ct);
}
