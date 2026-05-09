#pragma warning disable CS1591
namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/raw/auth")]
public class RawAuthController : ApiControllerBase
{
    private readonly IAuthRepository _authRepository;

    public RawAuthController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [HttpGet("users/by-email")]
    public IActionResult FindUserByEmail([FromQuery] string email)
        => ToActionResult(_authRepository.FindUserByEmail(email), Ok);

    [HttpGet("users/{userId:int}")]
    public IActionResult FindUserById(int userId)
        => ToActionResult(_authRepository.FindUserById(userId), Ok);

    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] User user)
        => ToActionResult(_authRepository.CreateUser(user));

    [HttpPost("sessions")]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request)
        => ToActionResult(
            _authRepository.CreateSession(
                request.UserId,
                request.Token,
                request.DeviceInfo,
                request.Browser,
                request.RemoteIpAddress),
            Ok);

    [HttpGet("sessions/by-token")]
    public IActionResult FindSessionByToken([FromQuery] string token)
        => ToActionResult(_authRepository.FindSessionByToken(token), Ok);

    [HttpGet("sessions/active")]
    public IActionResult IsSessionActive([FromQuery] string token)
        => ToActionResult(_authRepository.IsSessionActive(token), isActive => Ok(isActive));

    [HttpGet("users/{userId:int}/sessions")]
    public IActionResult FindSessionsByUserId(int userId)
        => ToActionResult(_authRepository.FindSessionsByUserId(userId), Ok);

    [HttpPut("sessions/{sessionId:int}/revoke")]
    public IActionResult RevokeSession(int sessionId)
        => ToActionResult(_authRepository.UpdateSessionToken(sessionId));

    [HttpPut("users/{userId:int}/invalidate-sessions")]
    public IActionResult InvalidateAllSessions(int userId)
        => ToActionResult(_authRepository.InvalidateAllSessions(userId));

    [HttpPost("password-reset-tokens")]
    public IActionResult SavePasswordResetToken([FromBody] SavePasswordResetTokenRequest request)
        => ToActionResult(
            _authRepository.SavePasswordResetToken(
                new PasswordResetToken
                {
                    User = new User { Id = request.UserId },
                    TokenHash = request.TokenHash,
                    ExpiresAt = request.ExpiresAt,
                    CreatedAt = request.CreatedAt,
                }));

    [HttpGet("password-reset-tokens/by-hash")]
    public IActionResult FindPasswordResetToken([FromQuery] string tokenHash)
        => ToActionResult(_authRepository.FindPasswordResetToken(tokenHash), Ok);

    [HttpPut("password-reset-tokens/{tokenId:int}/mark-used")]
    public IActionResult MarkPasswordResetTokenAsUsed(int tokenId)
        => ToActionResult(_authRepository.MarkPasswordResetTokenAsUsed(tokenId));

    [HttpDelete("password-reset-tokens/expired")]
    public IActionResult DeleteExpiredPasswordResetTokens()
        => ToActionResult(_authRepository.DeleteExpiredPasswordResetTokens());

    [HttpPut("users/{userId:int}/failed-attempts/increment")]
    public IActionResult IncrementFailedAttempts(int userId)
        => ToActionResult(_authRepository.IncrementFailedAttempts(userId));

    [HttpPut("users/{userId:int}/failed-attempts/reset")]
    public IActionResult ResetFailedAttempts(int userId)
        => ToActionResult(_authRepository.ResetFailedAttempts(userId));

    [HttpPut("users/{userId:int}/lock")]
    public IActionResult LockAccount(int userId, [FromBody] LockAccountRequest request)
        => ToActionResult(_authRepository.LockAccount(userId, request.LockoutEnd));

    [HttpPut("users/{userId:int}/password")]
    public IActionResult UpdatePassword(int userId, [FromBody] UpdatePasswordRequest request)
        => ToActionResult(_authRepository.UpdatePassword(userId, request.NewPasswordHash));

    public sealed class CreateSessionRequest
    {
        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string? DeviceInfo { get; set; }

        public string? Browser { get; set; }

        public string? RemoteIpAddress { get; set; }
    }

    public sealed class SavePasswordResetTokenRequest
    {
        public int UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public sealed class LockAccountRequest
    {
        public DateTime LockoutEnd { get; set; }
    }

    public sealed class UpdatePasswordRequest
    {
        public string NewPasswordHash { get; set; } = string.Empty;
    }
}
#pragma warning restore CS1591
