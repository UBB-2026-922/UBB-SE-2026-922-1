namespace BankingApp.Infrastructure.Http.Shared.Http;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Application.Shared.Http;
using BankingApp.Infrastructure.Http.Common.Logging;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides a thin HTTP client abstraction for desktop and other client-side code.
/// </summary>
public sealed partial class ApiClient : IApiClient, IDisposable
{
    private readonly Error? _configurationError;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private bool _disposed;

    public ApiClient(IConfiguration configuration, ILogger<ApiClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        string? baseUrl = configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _configurationError = Error.Failure("ApiClient.MissingBaseUrl", "ApiBaseUrl is missing from configuration.");
            _httpClient = new HttpClient();
            _logger.ApiBaseUrlMissing();
            return;
        }

        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public string? Token { get; private set; }

    public ErrorOr<Success> EnsureConfigured()
    {
        if (_configurationError is null)
        {
            return Result.Success;
        }

        return _configurationError.Value;
    }

    public void SetToken(string token)
    {
        Token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _logger.ApiTokenSet();
    }

    public void ClearToken()
    {
        Token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _logger.ApiTokenCleared();
    }

    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>("GET", endpoint, async ct => await _httpClient.GetAsync(endpoint, ct), cancellationToken);

    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>("POST", endpoint, async ct => await _httpClient.PostAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync("POST", endpoint, async ct => await _httpClient.PostAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>("PUT", endpoint, async ct => await _httpClient.PutAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync("PUT", endpoint, async ct => await _httpClient.PutAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        => SendSuccessAsync("DELETE", endpoint, async ct => await _httpClient.DeleteAsync(endpoint, ct), cancellationToken);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }

    private async Task<ErrorOr<TResponse>> SendAsync<TResponse>(
        string operation,
        string endpoint,
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(operation, endpoint, response, cancellationToken);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            if (result is null)
            {
                _logger.HttpEmptyResponse(operation, endpoint);
                return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
            }

            return result;
        }
        catch (HttpRequestException exception)
        {
            _logger.HttpRequestTransportFailed(exception, operation, endpoint);
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.HttpRequestCancelled(operation, endpoint);
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private async Task<ErrorOr<Success>> SendSuccessAsync(
        string operation,
        string endpoint,
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            return response.IsSuccessStatusCode ? Result.Success : await MapErrorAsync(operation, endpoint, response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.HttpRequestTransportFailed(exception, operation, endpoint);
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            _logger.HttpRequestCancelled(operation, endpoint);
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private async Task<Error> MapErrorAsync(string operation, string endpoint, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string details;
        try
        {
            details = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            details = string.Empty;
        }

        string description = string.IsNullOrWhiteSpace(details)
            ? response.ReasonPhrase ?? "Request failed."
            : details;

        _logger.HttpRequestFailed(operation, endpoint, (int)response.StatusCode, description);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(description: description),
            HttpStatusCode.Unauthorized => Error.Unauthorized(description: description),
            HttpStatusCode.Forbidden => Error.Forbidden(description: description),
            HttpStatusCode.NotFound => Error.NotFound(description: description),
            HttpStatusCode.Conflict => Error.Conflict(description: description),
            HttpStatusCode.UnprocessableEntity => Error.Validation(description: description),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(description: description),
            _ => Error.Failure(description: description),
        };
    }
}
