namespace BankingApp.Api.Controllers;

using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// TODO: add docs.
/// </summary>
/// <param name="authRepository"></param>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthRepository authRepository) : ApiController
{
    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpGet("users/by-email")]
    public IActionResult FindUserByEmail([FromQuery] string email)
        => ToActionResult(authRepository.FindUserByEmail(email), Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("users/{userId:int}")]
    public IActionResult FindUserById(int userId)
        => ToActionResult(authRepository.FindUserById(userId), Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] User user)
        => ToActionResult(authRepository.CreateUser(user));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("sessions")]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request)
        => ToActionResult(
            authRepository.CreateSession(
                request.UserId,
                request.Token,
                request.DeviceInfo,
                request.Browser,
                request.RemoteIpAddress),
            Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("sessions/by-token")]
    public IActionResult FindSessionByToken([FromQuery] string token)
        => ToActionResult(authRepository.FindSessionByToken(token), Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("sessions/active")]
    public IActionResult IsSessionActive([FromQuery] string token)
        => ToActionResult(authRepository.IsSessionActive(token), isActive => Ok(isActive));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("users/{userId:int}/sessions")]
    public IActionResult FindSessionsByUserId(int userId)
        => ToActionResult(authRepository.FindSessionsByUserId(userId), Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    [HttpPut("sessions/{sessionId:int}/revoke")]
    public IActionResult RevokeSession(int sessionId)
        => ToActionResult(authRepository.UpdateSessionToken(sessionId));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPut("users/{userId:int}/invalidate-sessions")]
    public IActionResult InvalidateAllSessions(int userId)
        => ToActionResult(authRepository.InvalidateAllSessions(userId));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("password-reset-tokens")]
    public IActionResult SavePasswordResetToken([FromBody] SavePasswordResetTokenRequest request)
        => ToActionResult(
            authRepository.SavePasswordResetToken(
                new PasswordResetToken
                {
                    User = new User { Id = request.UserId },
                    TokenHash = request.TokenHash,
                    ExpiresAt = request.ExpiresAt,
                    CreatedAt = request.CreatedAt,
                }));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="tokenHash"></param>
    /// <returns></returns>
    [HttpGet("password-reset-tokens/by-hash")]
    public IActionResult FindPasswordResetToken([FromQuery] string tokenHash)
        => ToActionResult(authRepository.FindPasswordResetToken(tokenHash), Ok);

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="tokenId"></param>
    /// <returns></returns>
    [HttpPut("password-reset-tokens/{tokenId:int}/mark-used")]
    public IActionResult MarkPasswordResetTokenAsUsed(int tokenId)
        => ToActionResult(authRepository.MarkPasswordResetTokenAsUsed(tokenId));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("password-reset-tokens/expired")]
    public IActionResult DeleteExpiredPasswordResetTokens()
        => ToActionResult(authRepository.DeleteExpiredPasswordResetTokens());

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPut("users/{userId:int}/failed-attempts/increment")]
    public IActionResult IncrementFailedAttempts(int userId)
        => ToActionResult(authRepository.IncrementFailedAttempts(userId));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPut("users/{userId:int}/failed-attempts/reset")]
    public IActionResult ResetFailedAttempts(int userId)
        => ToActionResult(authRepository.ResetFailedAttempts(userId));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("users/{userId:int}/lock")]
    public IActionResult LockAccount(int userId, [FromBody] LockAccountRequest request)
        => ToActionResult(authRepository.LockAccount(userId, request.LockoutEnd));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("users/{userId:int}/password")]
    public IActionResult UpdatePassword(int userId, [FromBody] UpdatePasswordRequest request)
        => ToActionResult(authRepository.UpdatePassword(userId, request.NewPasswordHash));

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public sealed class CreateSessionRequest
    {
        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string? Browser { get; set; }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string? RemoteIpAddress { get; set; }
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public sealed class SavePasswordResetTokenRequest
    {
        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public sealed class LockAccountRequest
    {
        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public DateTime LockoutEnd { get; set; }
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public sealed class UpdatePasswordRequest
    {
        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public string NewPasswordHash { get; set; } = string.Empty;
    }
}
