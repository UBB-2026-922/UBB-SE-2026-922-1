namespace BankingApp.Infrastructure.Http.Features.UserProfile.Services;

using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class ProfileService(IHttpClientFactory httpClientFactory, ILogger<ProfileService> logger) : IProfileService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<ProfileService> _logger = logger;

    public Task<ErrorOr<ProfileDto>> GetProfileAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<ProfileDto>(ApiEndpoints.Profile, _logger, ct);

    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Profile, request, _logger, ct);

    public Task<ErrorOr<bool>> VerifyPasswordAsync(string password, CancellationToken ct = default)
        => _http.PostErrorOrAsync<string, bool>(ApiEndpoints.VerifyPassword, password, _logger, ct);

    public Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.ChangePassword, request, _logger, ct);

    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Enable2Fa, request, _logger, ct);

    public Task<ErrorOr<Success>> Disable2FaAsync(CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.Disable2Fa, new { }, _logger, ct);

    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<NotificationPreferenceDto>>(ApiEndpoints.NotificationPreferences, _logger, ct);

    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences, CancellationToken ct = default)
        => _http.PutErrorOrAsync(ApiEndpoints.NotificationPreferences, preferences, _logger, ct);

    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync(CancellationToken ct = default)
        => _http.GetErrorOrAsync<List<SessionDto>>(ApiEndpoints.Sessions, _logger, ct);

    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId, CancellationToken ct = default)
        => _http.DeleteErrorOrAsync($"{ApiEndpoints.Sessions}/{sessionId}", _logger, ct);
}
