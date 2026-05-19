namespace BankingApp.Infrastructure.Http.Http;

using System.Net;
using BankingApp.Infrastructure.Http.Common.Logging;
using ErrorOr;
using Microsoft.Extensions.Logging;

internal static class HttpClientExtensions
{
    extension(HttpClient http)
    {
        internal async Task<ErrorOr<T>> GetErrorOrAsync<T>(
            string endpoint,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.GetAsync(endpoint, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("GET", endpoint, logger, ct);
                }

                T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
                if (result is null)
                {
                    logger?.HttpEmptyResponse("GET", endpoint);
                    return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
                }

                return result;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "GET", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("GET", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }

        internal async Task<ErrorOr<Success>> PostErrorOrAsync<TRequest>(
            string endpoint,
            TRequest body,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.PostAsJsonAsync(endpoint, body, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("POST", endpoint, logger, ct);
                }

                return Result.Success;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "POST", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("POST", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }

        internal async Task<ErrorOr<TResponse>> PostErrorOrAsync<TRequest, TResponse>(
            string endpoint,
            TRequest body,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.PostAsJsonAsync(endpoint, body, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("POST", endpoint, logger, ct);
                }

                TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
                if (result is null)
                {
                    logger?.HttpEmptyResponse("POST", endpoint);
                    return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
                }

                return result;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "POST", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("POST", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }

        internal async Task<ErrorOr<Success>> PutErrorOrAsync<TRequest>(
            string endpoint,
            TRequest body,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.PutAsJsonAsync(endpoint, body, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("PUT", endpoint, logger, ct);
                }

                return Result.Success;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "PUT", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("PUT", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }

        internal async Task<ErrorOr<TResponse>> PutErrorOrAsync<TRequest, TResponse>(
            string endpoint,
            TRequest body,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.PutAsJsonAsync(endpoint, body, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("PUT", endpoint, logger, ct);
                }

                TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
                if (result is null)
                {
                    logger?.HttpEmptyResponse("PUT", endpoint);
                    return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
                }

                return result;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "PUT", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("PUT", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }

        internal async Task<ErrorOr<Success>> DeleteErrorOrAsync(
            string endpoint,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            try
            {
                using HttpResponseMessage response = await http.DeleteAsync(endpoint, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.ToApiErrorAsync("DELETE", endpoint, logger, ct);
                }

                return Result.Success;
            }
            catch (HttpRequestException exception)
            {
                logger?.HttpRequestTransportFailed(exception, "DELETE", endpoint);
                return Error.Failure(description: exception.Message);
            }
            catch (OperationCanceledException)
            {
                logger?.HttpRequestCancelled("DELETE", endpoint);
                return Error.Unexpected("Api.RequestCancelled", "The API request was cancelled.");
            }
        }
    }

    private static async Task<Error> ToApiErrorAsync(
        this HttpResponseMessage response,
        string operation,
        string endpoint,
        ILogger? logger,
        CancellationToken ct)
    {
        string detail;
        try
        {
            detail = await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            detail = string.Empty;
        }

        string message = string.IsNullOrWhiteSpace(detail)
            ? response.ReasonPhrase ?? "Request failed."
            : detail;

        logger?.HttpRequestFailed(operation, endpoint, (int)response.StatusCode, message);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(description: message),
            HttpStatusCode.Unauthorized => Error.Unauthorized(description: message),
            HttpStatusCode.Forbidden => Error.Forbidden(description: message),
            HttpStatusCode.NotFound => Error.NotFound(description: message),
            HttpStatusCode.Conflict => Error.Conflict(description: message),
            HttpStatusCode.UnprocessableEntity => Error.Validation(description: message),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(description: message),
            _ => Error.Failure($"Api.{(int)response.StatusCode}", message),
        };
    }
}
