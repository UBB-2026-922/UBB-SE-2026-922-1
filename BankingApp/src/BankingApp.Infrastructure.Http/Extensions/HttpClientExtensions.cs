namespace BankingApp.Infrastructure.Http.Http;

using ErrorOr;

internal static class HttpClientExtensions
{
    extension(HttpClient http)
    {
        internal async Task<ErrorOr<T>> GetErrorOrAsync<T>(string endpoint,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.GetAsync(endpoint, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            if (result is null)
            {
                return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
            }

            return result;
        }

        internal async Task<ErrorOr<Success>> PostErrorOrAsync<TRequest>(string endpoint,
            TRequest body,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.PostAsJsonAsync(endpoint, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            return Result.Success;
        }

        internal async Task<ErrorOr<TResponse>> PostErrorOrAsync<TRequest, TResponse>(string endpoint,
            TRequest body,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.PostAsJsonAsync(endpoint, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
            if (result is null)
            {
                return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
            }

            return result;
        }

        internal async Task<ErrorOr<Success>> PutErrorOrAsync<TRequest>(string endpoint,
            TRequest body,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.PutAsJsonAsync(endpoint, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            return Result.Success;
        }

        internal async Task<ErrorOr<TResponse>> PutErrorOrAsync<TRequest, TResponse>(string endpoint,
            TRequest body,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.PutAsJsonAsync(endpoint, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
            if (result is null)
            {
                return Error.Failure("Api.EmptyResponse", "The API returned an empty response.");
            }

            return result;
        }

        internal async Task<ErrorOr<Success>> DeleteErrorOrAsync(string endpoint,
            CancellationToken ct = default)
        {
            using HttpResponseMessage response = await http.DeleteAsync(endpoint, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await response.ToApiErrorAsync(ct);
            }

            return Result.Success;
        }
    }

    private static async Task<Error> ToApiErrorAsync(this HttpResponseMessage response, CancellationToken ct)
    {
        string detail;
        try
        {
            detail = await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception)
        {
            detail = string.Empty;
        }

        string message = string.IsNullOrWhiteSpace(detail)
            ? response.ReasonPhrase ?? "Request failed."
            : detail;

        return Error.Failure($"Api.{(int)response.StatusCode}", message);
    }
}
