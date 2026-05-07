namespace BankingApp.Api.Middleware;

using Application.Repositories.Interfaces;
using Application.Services.Security;
using Logging;
using ErrorOr;
using System.Globalization;

/// <summary>
///     Middleware that validates bearer tokens and active sessions on non-public endpoints.
/// </summary>
public class SessionValidationMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private static readonly string[] _publicEndpointPrefixes = ["/api/auth/", "/swagger"];
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionValidationMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <returns>The result of the operation.</returns>
    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///     Validates the authorization token and session, then invokes the next middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="authRepository">The authentication repository used to verify sessions.</param>
    /// <param name="jsonWebTokenService">The JWT service used to extract and validate tokens.</param>
    /// <param name="logger">Logger for validation errors.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task Invoke(
        HttpContext context,
        IAuthRepository authRepository,
        IJsonWebTokenService jsonWebTokenService,
        ILogger<SessionValidationMiddleware> logger)
    {
        string? path = context.Request.Path.Value?.ToLower(CultureInfo.InvariantCulture);

        // Public endpoints, no token needed
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        // No token provided
        if (authHeader?.StartsWith(BearerPrefix, StringComparison.Ordinal) != true)
        {
            await RejectRequest(context, "No token provided.");
            return;
        }

        string token = authHeader[BearerPrefix.Length..];

        // Check if JWT valid
        ErrorOr<int> userIdResult = jsonWebTokenService.ExtractUserId(token);
        if (userIdResult.IsError)
        {
            logger.TokenValidationFailed(userIdResult.FirstError.Code, userIdResult.FirstError.Description);
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        // Check if session still active in the databaseContext
        ErrorOr<bool> sessionResult = authRepository.IsSessionActive(token);
        if (sessionResult.IsError)
        {
            logger.SessionLookupFailed(sessionResult.FirstError.Code, sessionResult.FirstError.Description);
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        // Store userId so controllers can use it
        context.Items["UserId"] = userIdResult.Value;
        await _next(context);
    }

    private static bool IsPublicEndpoint(string? path)
    {
        return path is not null &&
               Array.Exists(
                   _publicEndpointPrefixes,
                   prefix => path.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static async Task RejectRequest(HttpContext context, string error)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error });
    }
}
