namespace BankingApp.Infrastructure.Http.Features.RecurringPayments.Services;

using Contracts.Features.RecurringPayments.Dtos;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class RecurringPaymentService(IHttpClientFactory httpClientFactory, ILogger<RecurringPaymentService> logger) : IRecurringPaymentService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<RecurringPaymentService> _logger = logger;

    public Task<ErrorOr<List<RecurringPaymentResponse>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<RecurringPaymentResponse>>(ApiEndpoints.RecurringPayments.Base, _logger, ct);

    public Task<ErrorOr<RecurringPaymentResponse>> CreateAsync(CreateRecurringPaymentRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<CreateRecurringPaymentRequest, RecurringPaymentResponse>(ApiEndpoints.RecurringPayments.Base, request, _logger, ct);

    public Task<ErrorOr<Success>> PauseAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.RecurringPayments.PauseFull(id), new { }, _logger, ct);

    public Task<ErrorOr<Success>> ResumeAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.RecurringPayments.ResumeFull(id), new { }, _logger, ct);

    public Task<ErrorOr<Success>> CancelAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync(ApiEndpoints.RecurringPayments.ByIdFull(id), _logger, ct);
}
