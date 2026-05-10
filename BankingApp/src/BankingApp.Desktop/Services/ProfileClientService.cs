namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Profile;
using Application.Repositories.Interfaces;
using Application.Utilities;
using Domain.Entities;
using ErrorOr;
using Utilities;

/// <summary>
///     Implements <see cref="IProfileClientService" /> with desktop-side business logic and proxy repositories.
/// </summary>
internal sealed class ProfileClientService(IApiClient apiClient, IUserRepository userRepository) : IProfileClientService
{
    private readonly IApiClient _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public Task<ErrorOr<ProfileDto>> GetProfileAsync()
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<ProfileDto>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId.Value);
        return Task.FromResult<ErrorOr<ProfileDto>>(userResult.IsError ? userResult.FirstError : new ProfileDto(userResult.Value));
    }

    public Task<ErrorOr<Success>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        if (request.UserId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Validation(description: "User id is required."));
        }

        ErrorOr<User> userResult = _userRepository.FindById(request.UserId.Value);
        if (userResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(userResult.FirstError);
        }

        User user = userResult.Value;
        if (request.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Task.FromResult<ErrorOr<Success>>(Error.Validation("full_name_required", "Full name is required."));
            }

            user.FullName = request.FullName.Trim();
        }

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return Task.FromResult<ErrorOr<Success>>(Error.Validation("invalid_phone", "Phone number is invalid."));
            }

            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.DateOfBirth != null)
        {
            user.DateOfBirth = request.DateOfBirth;
        }

        if (request.Address != null)
        {
            user.Address = request.Address.Trim();
        }

        if (request.Nationality != null)
        {
            user.Nationality = request.Nationality.Trim();
        }

        if (request.PreferredLanguage != null)
        {
            if (string.IsNullOrWhiteSpace(request.PreferredLanguage))
            {
                return Task.FromResult<ErrorOr<Success>>(Error.Validation("preferred_language_required", "Preferred language is required."));
            }

            user.PreferredLanguage = request.PreferredLanguage.Trim();
        }

        return Task.FromResult(_userRepository.UpdateUser(user));
    }

    public async Task<ErrorOr<bool>> VerifyPasswordAsync(string password)
    {
        ErrorOr<ProfileDto> profileResult = await GetProfileAsync();
        if (profileResult.IsError || profileResult.Value.UserId is null)
        {
            return profileResult.IsError ? profileResult.FirstError : Error.Unauthorized(description: "User is not authenticated.");
        }

        ErrorOr<User> userResult = _userRepository.FindById(profileResult.Value.UserId.Value);
        if (userResult.IsError)
        {
            return userResult.FirstError;
        }

        if (userResult.Value.PasswordHash is null)
        {
            return false;
        }

        return await _apiClient.PostAsync<object, bool>("/api/raw/security/verify-hash", new
        {
            Input = password,
            Hash = userResult.Value.PasswordHash,
        });
    }

    public async Task<ErrorOr<Success>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        ErrorOr<User> userResult = _userRepository.FindById(request.UserId);
        if (userResult.IsError)
        {
            return userResult.FirstError;
        }

        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return Error.Validation("weak_password", "New password is too weak.");
        }

        if (userResult.Value.PasswordHash is null)
        {
            return Error.Validation("incorrect_password", "Current password is incorrect.");
        }

        ErrorOr<bool> verifyResult = await _apiClient.PostAsync<object, bool>("/api/raw/security/verify-hash", new
        {
            Input = request.CurrentPassword,
            Hash = userResult.Value.PasswordHash,
        });
        if (verifyResult.IsError)
        {
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return Error.Validation("incorrect_password", "Current password is incorrect.");
        }

        ErrorOr<string> hashResult = await _apiClient.PostAsync<object, string>("/api/raw/security/hash", new
        {
            Input = request.NewPassword,
        });
        if (hashResult.IsError)
        {
            return hashResult.FirstError;
        }

        return _userRepository.UpdatePassword(request.UserId, hashResult.Value);
    }

    public Task<ErrorOr<Success>> Enable2FaAsync(EnableTwoFaRequest request)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId.Value);
        if (userResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(userResult.FirstError);
        }

        userResult.Value.Enable2Fa(request.Method);
        return Task.FromResult(_userRepository.UpdateUser(userResult.Value));
    }

    public Task<ErrorOr<Success>> Disable2FaAsync()
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId.Value);
        if (userResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(userResult.FirstError);
        }

        userResult.Value.Disable2Fa();
        return Task.FromResult(_userRepository.UpdateUser(userResult.Value));
    }

    public Task<ErrorOr<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync()
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<NotificationPreferenceDto>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<NotificationPreference>> preferencesResult = _userRepository.GetNotificationPreferences(userId.Value);
        if (preferencesResult.IsError)
        {
            return Task.FromResult<ErrorOr<List<NotificationPreferenceDto>>>(preferencesResult.FirstError);
        }

        var preferences = preferencesResult.Value.Select(preference => new NotificationPreferenceDto
        {
            Id = preference.Id,
            UserId = preference.User?.Id ?? userId.Value,
            Category = preference.Category,
            PushEnabled = preference.PushEnabled,
            EmailEnabled = preference.EmailEnabled,
            SmsEnabled = preference.SmsEnabled,
            MinAmountThreshold = preference.MinAmountThreshold,
        }).ToList();

        return Task.FromResult<ErrorOr<List<NotificationPreferenceDto>>>(preferences);
    }

    public Task<ErrorOr<Success>> UpdateNotificationPreferencesAsync(List<NotificationPreferenceDto> preferences)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        var entities = preferences.Select(preference => new NotificationPreference
        {
            Id = preference.Id,
            User = new User { Id = userId.Value },
            Category = preference.Category,
            PushEnabled = preference.PushEnabled,
            EmailEnabled = preference.EmailEnabled,
            SmsEnabled = preference.SmsEnabled,
            MinAmountThreshold = preference.MinAmountThreshold,
        }).ToList();

        return Task.FromResult(_userRepository.UpdateNotificationPreferences(userId.Value, entities));
    }

    public Task<ErrorOr<List<SessionDto>>> GetSessionsAsync()
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<SessionDto>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<Session>> sessionsResult = _userRepository.GetActiveSessions(userId.Value);
        if (sessionsResult.IsError)
        {
            return Task.FromResult<ErrorOr<List<SessionDto>>>(sessionsResult.FirstError);
        }

        var sessions = sessionsResult.Value.Select(session => new SessionDto
        {
            Id = session.Id,
            DeviceInfo = session.DeviceInfo,
            Browser = session.Browser,
            IpAddress = session.IpAddress,
            LastActiveAt = session.LastActiveAt,
        }).ToList();

        return Task.FromResult<ErrorOr<List<SessionDto>>>(sessions);
    }

    public Task<ErrorOr<Success>> RevokeSessionAsync(int sessionId)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        return Task.FromResult(_userRepository.RevokeSession(userId.Value, sessionId));
    }
}
