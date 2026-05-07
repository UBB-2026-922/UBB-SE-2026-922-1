using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IProfileClientService
{
    Task<ErrorOr<ProfileDto>> GetProfileAsync();

    Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request);

    Task<ErrorOr<bool>> VerifyPasswordAsync(string password);

    Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request);

    Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request);

    Task<ErrorOr<Success>> Disable2FaAsync();

    Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync();

    Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences);

    Task<ErrorOr<List<SessionDto>>> GetSessionsAsync();

    Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId);
}
