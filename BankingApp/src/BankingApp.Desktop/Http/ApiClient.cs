namespace BankingApp.Desktop.Utilities;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.Common.Dtos;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides a thin wrapper around <see cref="HttpClient" /> for the application's API calls.
/// </summary>
public sealed partial class ApiClient : IApiClient, IDisposable
{
    private readonly Error? _configurationError;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiClient" /> class.
    /// </summary>
    public ApiClient(IConfiguration configuration, ILogger<ApiClient> logger)
    {
        _logger = logger;
        string? baseUrl = configuration["ApiBaseUrl"];
        if (baseUrl is null)
        {
            _configurationError = Error.Failure(
                "ApiClient.MissingBaseUrl",
                "ApiBaseUrl is missing from configuration.");
            _logger.ApiBaseUrlMissing();
            _httpClient = new HttpClient();
        }
        else
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }
    }

    /// <inheritdoc />
    public int? CurrentUserId { get; set; }

    /// <inheritdoc />
    public string? Token { get; private set; }

    /// <inheritdoc />
    public ErrorOr<Success> EnsureConfigured()
    {
        return _configurationError ?? ErrorOrFactory.From(Result.Success);
    }

    /// <inheritdoc />
    public int? GetCurrentUserId() => CurrentUserId;

    /// <inheritdoc />
    public void SetCurrentUserId(int userId) => CurrentUserId = userId;

    /// <inheritdoc />
    public void SetToken(string tokenStr)
    {
        Token = tokenStr;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenStr);
    }

    /// <inheritdoc />
    public void ClearToken()
    {
        Token = null;
        CurrentUserId = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, object? data)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, endpoint, CancellationToken.None);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>();
            return result is null
                ? Error.Failure(description: $"POST {endpoint} returned an empty response.")
                : result;
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"POST {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"POST {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode
                ? Result.Success
                : await MapErrorAsync(response, endpoint, CancellationToken.None);
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"POST {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"POST {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<TResponse>> GetAsync<TResponse>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, endpoint, cancellationToken);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return result ??
                   (ErrorOr<TResponse>)Error.Failure(description: $"GET {endpoint} returned an empty response.");
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"GET {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Failure(description: $"GET {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, endpoint, CancellationToken.None);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>();
            return result ??
                   (ErrorOr<TResponse>)Error.Failure(description: $"PUT {endpoint} returned an empty response.");
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"PUT {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"PUT {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest data)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode
                ? Result.Success
                : await MapErrorAsync(response, endpoint, CancellationToken.None);
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"PUT {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"PUT {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> DeleteAsync(string endpoint)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode
                ? Result.Success
                : await MapErrorAsync(response, endpoint, CancellationToken.None);
        }
        catch (HttpRequestException exception)
        {
            return Error.Failure(description: $"DELETE {endpoint} failed: {exception.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"DELETE {endpoint} was cancelled.");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    private static async Task<Error> MapErrorAsync(
        HttpResponseMessage response,
        string endpoint,
        CancellationToken cancellationToken)
    {
        string errorCode = string.Empty;
        string description;
        try
        {
            ApplicationErrorResponse? errorBody = await response.Content
                .ReadFromJsonAsync<ApplicationErrorResponse>(cancellationToken);
            if (errorBody is not null && !string.IsNullOrWhiteSpace(errorBody.Error))
            {
                description = errorBody.Error;
                errorCode = errorBody.ErrorCode;
            }
            else
            {
                description = $"Request to '{endpoint}' failed with status {(int)response.StatusCode}.";
            }
        }
        catch
        {
            description = $"Request to '{endpoint}' failed with status {(int)response.StatusCode}.";
        }

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(errorCode, description),
            HttpStatusCode.Unauthorized => Error.Unauthorized(errorCode, description),
            HttpStatusCode.Forbidden => Error.Forbidden(errorCode, description),
            HttpStatusCode.Conflict => Error.Conflict(errorCode, description),
            HttpStatusCode.NotFound => Error.NotFound(errorCode, description),
            HttpStatusCode.UnprocessableEntity => Error.Validation(errorCode, description),
            HttpStatusCode.TooManyRequests => Error.Failure(errorCode, description),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(errorCode, description),
            _ => Error.Failure(errorCode, description)
        };
    }
}
