namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.Features.UserProfile.Dtos;
using BankingApp.Application.Common.Utilities;
using ErrorOr;

/// <summary>
///     Implements <see cref="IProfileClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class ProfileClientService : IProfileClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileClientService" /> class.
    /// </summary>
    public ProfileClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public Task<ErrorOr<ProfileDto>> GetProfileAsync()
    {
        return _apiClient.GetAsync<ProfileDto>(ApiEndpoints.Profile);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.Profile, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password)
    {
        return _apiClient.PostAsync<string, bool>(ApiEndpoints.VerifyPassword, password);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.ChangePassword, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request)
    {
        return _apiClient.PutAsync(ApiEndpoints.Enable2Fa, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> Disable2FaAsync()
    {
        return _apiClient.PutAsync<object>(ApiEndpoints.Disable2Fa, new { });
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync()
    {
        return _apiClient.GetAsync<List<NotificationPreferenceDto>>(ApiEndpoints.NotificationPreferences);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences)
    {
        return _apiClient.PutAsync(ApiEndpoints.NotificationPreferences, preferences);
    }

    /// <inheritdoc />
    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync()
    {
        return _apiClient.GetAsync<List<SessionDto>>(ApiEndpoints.Sessions);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId)
    {
        return _apiClient.DeleteAsync($"{ApiEndpoints.Sessions}/{sessionId}");
    }
}
