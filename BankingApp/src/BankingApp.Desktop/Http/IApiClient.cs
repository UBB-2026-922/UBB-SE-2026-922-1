namespace BankingApp.Desktop.Utilities;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

/// <summary>
///     Defines the client-side boundary for authenticated API communication.
/// </summary>
public interface IApiClient
{
    /// <summary>Gets or sets the identifier of the currently authenticated user.</summary>
    public int? CurrentUserId { get; set; }

    /// <summary>Gets the currently configured bearer token.</summary>
    public string? Token { get; }

    /// <summary>Returns <see cref="Success" /> when the client is correctly configured.</summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>Gets the identifier of the currently authenticated user.</summary>
    public int? GetCurrentUserId();

    /// <summary>Sets the identifier of the currently authenticated user.</summary>
    public void SetCurrentUserId(int userId);

    /// <summary>Sets the bearer token used for authenticated requests.</summary>
    public void SetToken(string tokenStr);

    /// <summary>Clears the stored authentication state from the client.</summary>
    public void ClearToken();

    /// <summary>Sends a POST request and deserializes the response body into <typeparamref name="TResponse" />.</summary>
    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, object? data);

    /// <summary>Sends a POST request and returns <see cref="Success" /> on a 2xx response.</summary>
    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>Sends a GET request and deserializes the response body.</summary>
    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>Sends a PUT request and deserializes the response body into <typeparamref name="TResponse" />.</summary>
    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>Sends a PUT request and returns <see cref="Success" /> on a 2xx response.</summary>
    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>Sends a DELETE request and returns <see cref="Success" /> on a 2xx response.</summary>
    public Task<ErrorOr<Success>> DeleteAsync(string endpoint);
}
