namespace BankingApp.Api.Controllers;

using Application.Features.Authentication.Commands;
using Application.Features.Authentication.Models;
using Application.Features.PasswordReset.Commands;
using Application.Features.PasswordReset.Queries;
using Application.Features.UserRegistration.Commands;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
///     Handles authentication, registration, password reset, and two-factor endpoints.
/// </summary>
[ApiController]
[Route(ApiEndpoints.Auth.Base)]
public class AuthController : ApiControllerBase
{
    private const int DeviceInfoMaxLength = 255;
    private const int BrowserMaxLength = 100;
    private const int IpAddressMaxLength = 45;

    [HttpPost(ApiEndpoints.Auth.Login)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, GetSessionMetadata());
        return ToActionResult(await Sender.Send(command, cancellationToken), MapLoginSuccess);
    }

    [HttpPost(ApiEndpoints.Auth.Register)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(
            await Sender.Send(new RegisterCommand(request.Email, request.Password, request.FullName), cancellationToken));
    }

    [HttpPost(ApiEndpoints.Auth.VerifyOtp)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyOtpCommand(request.UserId, request.OtpCode, GetSessionMetadata());
        return ToActionResult(await Sender.Send(command, cancellationToken), MapLoginSuccess);
    }

    [HttpPost(ApiEndpoints.Auth.ForgotPassword)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await Sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost(ApiEndpoints.Auth.ResetPassword)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return ToActionResult(
            await Sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), cancellationToken));
    }

    [HttpPost(ApiEndpoints.Auth.Logout)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = AuthHeaderNames.Authorization)] string authorization,
        CancellationToken cancellationToken)
    {
        if (!TryExtractBearerToken(authorization, out string token))
        {
            return Problem(detail: "No token provided.", statusCode: StatusCodes.Status400BadRequest);
        }

        return ToActionResult(await Sender.Send(new LogoutCommand(token), cancellationToken));
    }

    [HttpPost(ApiEndpoints.Auth.ResendOtp)]
    public async Task<IActionResult> ResendOtp(
        [FromQuery] int userId,
        [FromQuery] string method = "email",
        CancellationToken cancellationToken = default)
    {
        await Sender.Send(new ResendOtpCommand(userId, method), cancellationToken);
        return Ok(new { message = "If the user exists, a new code has been sent." });
    }

    [HttpPost(ApiEndpoints.Auth.VerifyResetToken)]
    public async Task<IActionResult> VerifyResetToken(
        [FromBody] VerifyResetTokenRequest request,
        CancellationToken cancellationToken)
    {
        return ToActionResult(await Sender.Send(new VerifyResetTokenQuery(request.Token), cancellationToken));
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        string forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        return !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',').First().Trim()
            : context.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetBrowserName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
        {
            return "Firefox";
        }

        if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        return "Unknown Browser";
    }

    private static string? TrimToMaxLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static bool TryExtractBearerToken(string authorization, out string token)
    {
        token = string.Empty;
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(AuthHeaderNames.BearerPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        token = authorization[AuthHeaderNames.BearerPrefix.Length..];
        return !string.IsNullOrWhiteSpace(token);
    }

    private IActionResult MapLoginSuccess(LoginSuccess success)
    {
        return success switch
        {
            FullLogin full => Ok(new LoginSuccessResponse { UserId = full.UserId, Token = full.Token }),
            RequiresTwoFactor tfa => Ok(new LoginSuccessResponse { UserId = tfa.UserId, Requires2Fa = true }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                Problem(detail: "Unexpected login result type.", statusCode: StatusCodes.Status500InternalServerError))
        };
    }

    private SessionMetadata GetSessionMetadata()
    {
        string? userAgent = TrimToMaxLength(Request.Headers.UserAgent.ToString(), DeviceInfoMaxLength);
        return new SessionMetadata
        {
            DeviceInfo = userAgent,
            Browser = TrimToMaxLength(GetBrowserName(userAgent), BrowserMaxLength),
            IpAddress = TrimToMaxLength(GetClientIpAddress(HttpContext), IpAddressMaxLength),
        };
    }
}
