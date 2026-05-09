namespace BankingApp.Desktop.ProxyRepositories;

using System.Collections.Generic;
using Application.Repositories.Interfaces;
using Domain.Entities;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class UserProxyRepository : IUserRepository
{
    private readonly IApiClient _apiClient;

    public UserProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ErrorOr<User> FindById(int userId)
        => _apiClient.GetAsync<User>($"/api/raw/users/{userId}").GetAwaiter().GetResult();

    public ErrorOr<Success> UpdateUser(User user)
        => _apiClient.PutAsync("/api/raw/users", user).GetAwaiter().GetResult();

    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
        => _apiClient.PutAsync($"/api/raw/users/{userId}/password", new UpdatePasswordRequest { NewPasswordHash = newPasswordHash })
            .GetAwaiter()
            .GetResult();

    public ErrorOr<List<Session>> GetActiveSessions(int userId)
        => _apiClient.GetAsync<List<Session>>($"/api/raw/users/{userId}/sessions").GetAwaiter().GetResult();

    public ErrorOr<Success> RevokeSession(int userId, int sessionId)
        => _apiClient.DeleteAsync($"/api/raw/users/{userId}/sessions/{sessionId}").GetAwaiter().GetResult();

    public ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId)
        => _apiClient.GetAsync<List<NotificationPreference>>($"/api/raw/users/{userId}/notification-preferences").GetAwaiter().GetResult();

    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences)
        => _apiClient.PutAsync($"/api/raw/users/{userId}/notification-preferences", preferences).GetAwaiter().GetResult();

    private sealed class UpdatePasswordRequest
    {
        public string NewPasswordHash { get; set; } = string.Empty;
    }
}
