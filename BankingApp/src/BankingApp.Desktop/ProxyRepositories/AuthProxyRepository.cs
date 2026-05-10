namespace BankingApp.Desktop.ProxyRepositories;

using System;
using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Application.Services.Login;
using BankingApp.Desktop.Utilities;
using Domain.Entities;
using ErrorOr;

internal sealed class AuthProxyRepository : IAuthRepository
{
    private readonly IApiClient _apiClient;

    public AuthProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<User> FindUserByEmail(string email)
        => _apiClient.GetAsync<User>($"/api/auth/users/by-email?email={Uri.EscapeDataString(email)}")
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> CreateUser(User user)
        => _apiClient.PostAsync("/api/auth/users", user).GetAwaiter().GetResult();

    public ErrorOr<Session> CreateSession(
        int userId,
        string token,
        string? deviceInfo,
        string? browser,
        string? remoteIpAddress)
        => _apiClient.PostAsync<CreateSessionRequest, Session>(
                "/api/auth/sessions",
                new CreateSessionRequest
                {
                    UserId = userId,
                    Token = token,
                    DeviceInfo = deviceInfo,
                    Browser = browser,
                    RemoteIpAddress = remoteIpAddress,
                })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Session> FindSessionByToken(string token)
        => _apiClient.GetAsync<Session>($"/api/auth/sessions/by-token?token={Uri.EscapeDataString(token)}")
            .GetAwaiter()
            .GetResult();

    public ErrorOr<bool> IsSessionActive(string token)
        => _apiClient.GetAsync<bool>($"/api/auth/sessions/active?token={Uri.EscapeDataString(token)}")
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> SavePasswordResetToken(PasswordResetToken token)
        => _apiClient.PostAsync(
                "/api/auth/password-reset-tokens",
                new SavePasswordResetTokenRequest
                {
                    UserId = token.User?.Id ?? 0,
                    TokenHash = token.TokenHash,
                    ExpiresAt = token.ExpiresAt,
                    CreatedAt = token.CreatedAt,
                })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<PasswordResetToken> FindPasswordResetToken(string tokenHash)
        => _apiClient.GetAsync<PasswordResetToken>(
                $"/api/auth/password-reset-tokens/by-hash?tokenHash={Uri.EscapeDataString(tokenHash)}")
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> MarkPasswordResetTokenAsUsed(int tokenId)
        => _apiClient.PutAsync($"/api/auth/password-reset-tokens/{tokenId}/mark-used", new { })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> DeleteExpiredPasswordResetTokens()
        => _apiClient.DeleteAsync("/api/auth/password-reset-tokens/expired").GetAwaiter().GetResult();

    public ErrorOr<Success> InvalidateAllSessions(int userId)
        => _apiClient.PutAsync($"/api/auth/users/{userId}/invalidate-sessions", new { })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<User> FindUserById(int id)
        => _apiClient.GetAsync<User>($"/api/auth/users/{id}").GetAwaiter().GetResult();

    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
        => _apiClient.PutAsync(
                $"/api/auth/users/{userId}/password",
                new UpdatePasswordRequest { NewPasswordHash = newPasswordHash })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<List<Session>> FindSessionsByUserId(int userId)
        => _apiClient.GetAsync<List<Session>>($"/api/auth/users/{userId}/sessions").GetAwaiter().GetResult();

    public ErrorOr<Success> UpdateSessionToken(int sessionId)
        => _apiClient.PutAsync($"/api/auth/sessions/{sessionId}/revoke", new { }).GetAwaiter().GetResult();

    public ErrorOr<Success> IncrementFailedAttempts(int userId)
        => _apiClient.PutAsync($"/api/auth/users/{userId}/failed-attempts/increment", new { })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> ResetFailedAttempts(int userId)
        => _apiClient.PutAsync($"/api/auth/users/{userId}/failed-attempts/reset", new { })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
        => _apiClient.PutAsync(
                $"/api/auth/users/{userId}/lock",
                new LockAccountRequest { LockoutEnd = lockoutEnd })
            .GetAwaiter()
            .GetResult();

    private sealed class CreateSessionRequest
    {
        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string? DeviceInfo { get; set; }

        public string? Browser { get; set; }

        public string? RemoteIpAddress { get; set; }
    }

    private sealed class SavePasswordResetTokenRequest
    {
        public int UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    private sealed class UpdatePasswordRequest
    {
        public string NewPasswordHash { get; set; } = string.Empty;
    }

    private sealed class LockAccountRequest
    {
        public DateTime LockoutEnd { get; set; }
    }
}
