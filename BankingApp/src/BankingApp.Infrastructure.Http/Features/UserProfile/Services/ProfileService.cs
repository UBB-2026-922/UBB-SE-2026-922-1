namespace BankingApp.Infrastructure.Http.Features.UserProfile.Services;

using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Contracts.Http;
using ErrorOr;

public sealed class ProfileService(IHttpClientFactory httpClientFactory) : IProfileService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<ProfileDto>> GetProfileAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<ProfileDto>(ApiEndpoints.Profile, ct);

    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Profile, request, ct);

    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password, CancellationToken ct = default)
        => _http.PostErrorOrAsync<string, bool>(ApiEndpoints.VerifyPassword, password, ct);

    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.ChangePassword, request, ct);

    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Enable2Fa, request, ct);

    public Task<ErrorOr<Success>> Disable2FaAsync(CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Disable2Fa, new { }, ct);

    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<NotificationPreferenceDto>>(ApiEndpoints.NotificationPreferences, ct);

    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.NotificationPreferences, preferences, ct);

    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<SessionDto>>(ApiEndpoints.Sessions, ct);

    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync($"{ApiEndpoints.Sessions}/{sessionId}", ct);
}
