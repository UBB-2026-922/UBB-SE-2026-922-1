namespace BankingApp.Infrastructure.Http.Features.Cards.Services;

using Contracts.Features.Cards.Dtos;
using Contracts.Features.Cards.Services;
using Contracts.Http;
using ErrorOr;

public sealed class CardService(IHttpClientFactory httpClientFactory) : ICardService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<CardDetailsDto>>(ApiEndpoints.Cards, ct);

    public Task<ErrorOr<CardDetailsDto>> IssueCardAsync(IssueCardRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<IssueCardRequest, CardDetailsDto>(ApiEndpoints.Cards, request, ct);

    public Task<ErrorOr<Success>> FreezeCardAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync($"{ApiEndpoints.Cards}/{id}/freeze", new { }, ct);

    public Task<ErrorOr<Success>> UnfreezeCardAsync(int id, CancellationToken ct = default)
        => _http.PutErrorOrAsync($"{ApiEndpoints.Cards}/{id}/unfreeze", new { }, ct);

    public Task<ErrorOr<Success>> CancelCardAsync(int id, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync($"{ApiEndpoints.Cards}/{id}", ct);
}
