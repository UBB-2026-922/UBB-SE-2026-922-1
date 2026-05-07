using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class ProfileClientService : IProfileClientService
{
    private readonly IApiClient _apiClient;

    public ProfileClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public Task<ErrorOr<ProfileDto>> GetProfileAsync()
    {
        return _apiClient.GetAsync<ProfileDto>(ApiEndpoints.Profile);
    }

    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.Profile, request);
    }

    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password)
    {
        return _apiClient.PostAsync<string, bool>(ApiEndpoints.VerifyPassword, password);
    }

    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.ChangePassword, request);
    }

    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.Enable2Fa, request);
    }

    public Task<ErrorOr<Success>> Disable2FaAsync()
    {
        return _apiClient.PutAsync<object>(ApiEndpoints.Disable2Fa, new { });
    }

    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync()
    {
        return _apiClient.GetAsync<List<NotificationPreferenceDto>>(ApiEndpoints.NotificationPreferences);
    }

    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences)
    {
        return _apiClient.PutAsync(ApiEndpoints.NotificationPreferences, preferences);
    }

    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync()
    {
        return _apiClient.GetAsync<List<SessionDto>>(ApiEndpoints.Sessions);
    }

    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.Sessions}/{sessionId}");
    }
}
