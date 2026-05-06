// <copyright file="ProfileService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ProfileService class.
// </summary>

using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Application.Logging;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Utilities;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Profile;

/// <summary>
///     Provides user profile management operations including personal info, passwords, 2FA, and notifications.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IHashService _hashService;
    private readonly ILogger<ProfileService> _logger;
    private readonly IUserRepository _userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileService" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="logger">The _logger.</param>
    /// <returns>The result of the operation.</returns>
    public ProfileService(IUserRepository userRepository, IHashService hashService, ILogger<ProfileService> logger)
    {
        _userRepository = userRepository;
        _hashService = hashService;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<ProfileInfo> GetProfile(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.ProfileFetchUserNotFound(userId);
            return userResult.FirstError;
        }

        return new ProfileInfo(userResult.Value);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePersonalInfo(UpdateProfileRequest request)
    {
        if (request.UserId == null) return ProfileErrors.UserIdRequired;

        int userId = request.UserId.Value;
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.ProfileUpdateUserNotFound(userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        if (request.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName)) return ProfileErrors.FullNameRequired;

            user.FullName = request.FullName.Trim();
        }

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber)) return ProfileErrors.InvalidPhone;

            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.DateOfBirth != null) user.DateOfBirth = request.DateOfBirth;

        if (request.Address != null) user.Address = request.Address.Trim();

        if (request.Nationality != null) user.Nationality = request.Nationality.Trim();

        if (request.PreferredLanguage != null)
        {
            if (string.IsNullOrWhiteSpace(request.PreferredLanguage)) return ProfileErrors.PreferredLanguageRequired;

            user.PreferredLanguage = request.PreferredLanguage.Trim();
        }

        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.ProfileUpdateFailed(userId);
            return UserErrors.UpdateFailed;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> ChangePassword(ChangePasswordRequest request)
    {
        ErrorOr<User> userResult = _userRepository.FindById(request.UserId);
        if (userResult.IsError)
        {
            _logger.PasswordChangeUserNotFound(request.UserId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        if (!ValidationUtilities.IsStrongPassword(request.NewPassword)) return ProfileErrors.WeakPasswordChange;

        if (user.PasswordHash is null)
        {
            _logger.PasswordChangeOAuthOnlyRejected(user.Id);
            // Use the generic password failure so password checks do not reveal whether a local password exists.
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<bool> verifyResult = _hashService.Verify(request.CurrentPassword, user.PasswordHash);
        if (verifyResult.IsError)
        {
            _logger.PasswordChangeHashVerificationFailed(user.Id);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            _logger.PasswordChangeIncorrectCurrentPassword(user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<string> newHashResult = _hashService.GetHash(request.NewPassword);
        if (newHashResult.IsError)
        {
            _logger.PasswordChangeHashGenerationFailed(user.Id);
            return newHashResult.FirstError;
        }

        if (_userRepository.UpdatePassword(user.Id, newHashResult.Value).IsError)
        {
            _logger.PasswordUpdateFailed(user.Id);
            return UserErrors.PasswordUpdateFailed;
        }

        _logger.PasswordChangedSuccessfully(user.Id);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="method">The method value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Enable2Fa(int userId, TwoFactorMethod method)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.EnableTwoFactorUserNotFound(userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Enable2Fa(method);
        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.EnableTwoFactorFailed(userId);
            return UserErrors.Enable2FaFailed;
        }

        _logger.EnableTwoFactorSucceeded(userId, method);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Disable2Fa(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.DisableTwoFactorUserNotFound(userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Disable2Fa();
        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.DisableTwoFactorFailed(userId);
            return UserErrors.Disable2FaFailed;
        }

        _logger.DisableTwoFactorSucceeded(userId);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<NotificationPreferenceDataTransferObject>> GetNotificationPreferences(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.NotificationPreferencesFetchUserNotFound(userId);
            return userResult.FirstError;
        }

        ErrorOr<List<NotificationPreference>> preferencesResult = _userRepository.GetNotificationPreferences(userId);
        if (preferencesResult.IsError)
        {
            _logger.NotificationPreferencesFetchFailed(userId, preferencesResult.FirstError.Description);
            return preferencesResult.FirstError;
        }

        return preferencesResult.Value
            .Select(preference => new NotificationPreferenceDataTransferObject
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = preference.Category,
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold
            })
            .ToList();
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="preferences">The preferences value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdateNotificationPreferences(
        int userId,
        List<NotificationPreferenceDataTransferObject> preferences)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.NotificationPreferencesUpdateUserNotFound(userId);
            return userResult.FirstError;
        }

        var entities = preferences
            .Select(preference => new NotificationPreference
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = preference.Category,
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold
            })
            .ToList();
        if (_userRepository.UpdateNotificationPreferences(userId, entities).IsError)
        {
            _logger.NotificationPreferencesUpdateFailed(userId);
            return UserErrors.NotificationPreferencesUpdateFailed;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="password">The password value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> VerifyPassword(int userId, string password)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.PasswordVerificationUserNotFound(userId);
            return userResult.FirstError;
        }

        if (userResult.Value.PasswordHash is null)
        {
            _logger.PasswordVerificationOAuthOnlyRejected(userId);
            // Return the same outcome as any other non-matching password for added security.
            return false;
        }

        return _hashService.Verify(password, userResult.Value.PasswordHash);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<SessionDataTransferObject>> GetActiveSessions(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.GetSessionsUserNotFound(userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Session>> sessionsResult = _userRepository.GetActiveSessions(userId);
        if (sessionsResult.IsError)
        {
            _logger.GetSessionsFailed(userId, sessionsResult.FirstError.Description);
            return sessionsResult.FirstError;
        }

        return sessionsResult.Value
            .Select(session => new SessionDataTransferObject
            {
                Id = session.Id,
                DeviceInfo = session.DeviceInfo,
                Browser = session.Browser,
                IpAddress = session.IpAddress,
                LastActiveAt = session.LastActiveAt,
                ExpiresAt = session.ExpiresAt,
                CreatedAt = session.CreatedAt
            })
            .ToList();
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeSession(int userId, int sessionId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.RevokeSessionUserNotFound(userId);
            return userResult.FirstError;
        }

        ErrorOr<Success> result = _userRepository.RevokeSession(userId, sessionId);
        if (result.IsError)
        {
            _logger.RevokeSessionFailed(sessionId, userId);
            return result.FirstError;
        }

        _logger.SessionRevoked(sessionId, userId);
        return Result.Success;
    }
}
