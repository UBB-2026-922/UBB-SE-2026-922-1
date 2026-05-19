namespace BankingApp.Infrastructure.Http.Features.Beneficiaries.Services;

using Contracts.Features.Beneficiaries.Dtos;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class BeneficiaryService(IHttpClientFactory httpClientFactory, ILogger<BeneficiaryService> logger) : IBeneficiaryService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<BeneficiaryService> _logger = logger;

    public Task<ErrorOr<List<BeneficiaryDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<BeneficiaryDto>>(ApiEndpoints.Beneficiaries.Base, _logger, ct);

    public async Task<ErrorOr<BeneficiaryDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ErrorOr<List<BeneficiaryDto>> result = await GetAllAsync(ct);
        if (result.IsError)
        {
            return result.Errors;
        }

        BeneficiaryDto? beneficiary = result.Value.FirstOrDefault(beneficiaryDto => beneficiaryDto.Id == id);

        if (beneficiary is null)
        {
            return Error.NotFound(
                code: "Beneficiary.NotFound",
                description: $"Beneficiary {id} was not found.");
        }

        return beneficiary;
    }

    public Task<ErrorOr<Success>> CreateAsync(CreateBeneficiaryRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.Beneficiaries.Base, request, _logger, ct);

    public Task<ErrorOr<Success>> UpdateAsync(int id, UpdateBeneficiaryRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Beneficiaries.ByIdFull(id), request, _logger, ct);

    public Task<ErrorOr<Success>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync(ApiEndpoints.Beneficiaries.ByIdFull(id), _logger, ct);
}
