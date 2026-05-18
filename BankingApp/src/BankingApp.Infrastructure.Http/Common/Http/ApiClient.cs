namespace BankingApp.Infrastructure.Http.Common.Http;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Application.Common.Http;
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
    private bool _disposed;

    public ApiClient(IConfiguration configuration, ILogger<ApiClient> logger)
    {
        _ = logger ?? throw new ArgumentNullException(nameof(logger));

        string? baseUrl = configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _configurationError = Error.Failure("ApiClient.MissingBaseUrl", "ApiBaseUrl is missing from configuration.");
            _httpClient = new HttpClient();
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
    }

    public void ClearToken()
    {
        Token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(async ct => await _httpClient.GetAsync(endpoint, ct), cancellationToken);

    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(async ct => await _httpClient.PostAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync(async ct => await _httpClient.PostAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(async ct => await _httpClient.PutAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest? data, CancellationToken cancellationToken = default)
        => SendSuccessAsync(async ct => await _httpClient.PutAsJsonAsync(endpoint, data, ct), cancellationToken);

    public Task<ErrorOr<Success>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        => SendSuccessAsync(async ct => await _httpClient.DeleteAsync(endpoint, ct), cancellationToken);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }

    private static async Task<ErrorOr<TResponse>> SendAsync<TResponse>(
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, cancellationToken);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return result is null
                ? Error.Failure("Api.EmptyResponse", "The API returned an empty response.")
                : result;
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private static async Task<ErrorOr<Success>> SendSuccessAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await send(cancellationToken);
            return response.IsSuccessStatusCode ? Result.Success : await MapErrorAsync(response, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: exception.Message);
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
        }
    }

    private static async Task<Error> MapErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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
