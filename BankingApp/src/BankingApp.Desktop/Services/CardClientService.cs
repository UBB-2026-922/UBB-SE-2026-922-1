namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.Features.Cards.Dtos;
using BankingApp.Desktop.Utilities;
using ErrorOr;

/// <summary>Implements <see cref="ICardClientService" /> using the shared desktop API client.</summary>
internal sealed class CardClientService : ICardClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>Initializes a new instance of the <see cref="CardClientService" /> class.</summary>
    public CardClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<CardDetailsDto>>> GetCardsAsync()
    {
        return _apiClient.GetAsync<List<CardDetailsDto>>(ApiEndpoints.Cards);
    }

    /// <inheritdoc />
    public Task<ErrorOr<CardDetailsDto>> IssueCardAsync(IssueCardRequest request)
    {
        return _apiClient.PostAsync<IssueCardRequest, CardDetailsDto>(ApiEndpoints.Cards, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> FreezeCardAsync(int cardId)
    {
        return _apiClient.PutAsync<object>($"{ApiEndpoints.Cards}/{cardId}/freeze", new { });
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> UnfreezeCardAsync(int cardId)
    {
        return _apiClient.PutAsync<object>($"{ApiEndpoints.Cards}/{cardId}/unfreeze", new { });
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> CancelCardAsync(int cardId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.Cards}/{cardId}");
    }
}
