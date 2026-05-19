namespace BankingApp.Desktop.Features.Authentication;

using System.Net.Http.Headers;
using Contracts.Http;

/// <summary>Adds the desktop session bearer token to shared HTTP infrastructure requests.</summary>
public sealed class AuthenticationSessionTokenHandler(IAuthenticationSession session) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(session.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthHeaderNames.BearerScheme, session.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
