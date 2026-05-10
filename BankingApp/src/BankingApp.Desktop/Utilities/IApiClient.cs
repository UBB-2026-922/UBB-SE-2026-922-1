namespace BankingApp.Desktop.Utilities;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

/// <summary>
///     HTTP transport layer for proxy repositories. Extends <see cref="ICurrentSession" /> so that
///     the single <see cref="ApiClient" /> singleton can satisfy both interfaces.
/// </summary>
public interface IApiClient : ICurrentSession
{
    /// <summary>
    ///     Sends a POST request and deserializes the response body into <typeparamref name="TResponse" />.
    /// </summary>
    public Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, object? data);

    /// <summary>
    ///     Sends a POST request and returns <see cref="Success" /> when the server responds with a 2xx status.
    /// </summary>
    public Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>
    ///     Sends a GET request and deserializes the response body.
    /// </summary>
    public Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends a PUT request and deserializes the response body into <typeparamref name="TResponse" />.
    /// </summary>
    public Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>
    ///     Sends a PUT request and returns <see cref="Success" /> when the server responds with a 2xx status.
    /// </summary>
    public Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>
    ///     Sends a DELETE request and returns <see cref="Success" /> when the server responds with a 2xx status.
    /// </summary>
    public Task<ErrorOr<Success>> DeleteAsync(string endpoint);
}
