// <copyright file="ProfileService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ProfileService class.
// </summary>

using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Enums;
using BankApp.Application.Mapping;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Security;
using BankApp.Application.Utilities;
using BankApp.Domain.Entities;
using BankApp.Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Profile;

/// <summary>
///     Provides user profile management operations including personal info, passwords, 2FA, OAuth, and notifications.
/// </summary>
public class ProfileService : IProfileService
{
    private const string GoogleOAuthProvider = "Google";
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
            _logger.LogWarning("Profile fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        return new ProfileInfo(userResult.Value);
    }

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePersonalInfo(UpdateProfileRequest request)
    {
        if (request.UserId == null)
        {
            return ProfileErrors.UserIdRequired;
        }

        int userId = request.UserId.Value;
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("Profile update failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        if (request.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return ProfileErrors.FullNameRequired;
            }

            user.FullName = request.FullName.Trim();
        }

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return ProfileErrors.InvalidPhone;
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
                return ProfileErrors.PreferredLanguageRequired;
            }

            user.PreferredLanguage = request.PreferredLanguage.Trim();
        }

        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.LogError("Profile update failed for user {UserId}.", userId);
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
            _logger.LogWarning("Password change failed: user {UserId} not found.", request.UserId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return ProfileErrors.WeakPasswordChange;
        }

        ErrorOr<bool> verifyResult = _hashService.Verify(request.CurrentPassword, user.PasswordHash);
        if (verifyResult.IsError)
        {
            _logger.LogError("Hash verification threw during password change for user {UserId}.", user.Id);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            _logger.LogWarning("Password change failed for user {UserId}: incorrect current password.", user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<string> newHashResult = _hashService.GetHash(request.NewPassword);
        if (newHashResult.IsError)
        {
            _logger.LogError("Hash generation failed during password change for user {UserId}.", user.Id);
            return newHashResult.FirstError;
        }

        if (_userRepository.UpdatePassword(user.Id, newHashResult.Value).IsError)
        {
            _logger.LogError("Password update failed for user {UserId}.", user.Id);
            return UserErrors.PasswordUpdateFailed;
        }

        _logger.LogInformation("Password changed successfully for user {UserId}.", user.Id);
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
            _logger.LogWarning("Enable 2FA failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Enable2Fa(DomainEnumMapper.ToDomain(method));
        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.LogError("Failed to enable 2FA for user {UserId}.", userId);
            return UserErrors.Enable2FaFailed;
        }

        _logger.LogInformation("2FA enabled for user {UserId} via {Method}.", userId, method);
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
            _logger.LogWarning("Disable 2FA failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Disable2Fa();
        if (_userRepository.UpdateUser(user).IsError)
        {
            _logger.LogError("Failed to disable 2FA for user {UserId}.", userId);
            return UserErrors.Disable2FaFailed;
        }

        _logger.LogInformation("2FA disabled for user {UserId}.", userId);
        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<OAuthLinkDataTransferObject>> GetOAuthLinks(int userId)
    {
        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OAuth links fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = _userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch OAuth links for user {UserId}: {Error}",
                userId,
                linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        return linksResult.Value
            .Select(oauthLink => new OAuthLinkDataTransferObject
            {
                Id = oauthLink.Id,
                Provider = oauthLink.Provider,
                ProviderEmail = oauthLink.ProviderEmail,
                LinkedAt = oauthLink.LinkedAt,
            })
            .ToList();
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="provider">The provider value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> LinkOAuth(int userId, string provider)
    {
        if (!IsSupportedOAuthProvider(provider))
        {
            return ProfileErrors.UnsupportedOAuthProvider;
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OAuth link failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = _userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch OAuth links for user {UserId}: {Error}",
                userId,
                linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        if (linksResult.Value.Any(link => string.Equals(
                link.Provider,
                GoogleOAuthProvider,
                StringComparison.OrdinalIgnoreCase)))
        {
            return AuthErrors.OAuthAlreadyLinked;
        }

        var providerUserId = $"local:{userId}:{GoogleOAuthProvider}";
        ErrorOr<Success> result = _userRepository.SaveOAuthLink(
            userId,
            GoogleOAuthProvider,
            providerUserId,
            userResult.Value.Email);
        if (result.IsError)
        {
            _logger.LogError(
                "Failed to link Google OAuth for user {UserId}: {Error}",
                userId,
                result.FirstError.Description);
            return result.FirstError;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="provider">The provider value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UnlinkOAuth(int userId, string provider)
    {
        if (!IsSupportedOAuthProvider(provider))
        {
            return ProfileErrors.UnsupportedOAuthProvider;
        }

        ErrorOr<User> userResult = _userRepository.FindById(userId);
        if (userResult.IsError)
        {
            _logger.LogWarning("OAuth unlink failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = _userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch OAuth links for user {UserId}: {Error}",
                userId,
                linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        OAuthLink? link = linksResult.Value.FirstOrDefault(oauthLink =>
            string.Equals(oauthLink.Provider, GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            return AuthErrors.OAuthLinkNotFound;
        }

        ErrorOr<Success> result = _userRepository.DeleteOAuthLink(link.Id);
        if (result.IsError)
        {
            _logger.LogError(
                "Failed to unlink Google OAuth for user {UserId}: {Error}",
                userId,
                result.FirstError.Description);
            return result.FirstError;
        }

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
            _logger.LogWarning("Notification preferences fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<NotificationPreference>> preferencesResult = _userRepository.GetNotificationPreferences(userId);
        if (preferencesResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch notification preferences for user {UserId}: {Error}",
                userId,
                preferencesResult.FirstError.Description);
            return preferencesResult.FirstError;
        }

        return preferencesResult.Value
            .Select(preference => new NotificationPreferenceDataTransferObject
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = DomainEnumMapper.ToApplication(preference.Category),
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold,
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
            _logger.LogWarning("Notification preferences update failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        List<NotificationPreference> entities = preferences
            .Select(preference => new NotificationPreference
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = DomainEnumMapper.ToDomain(preference.Category),
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold,
            })
            .ToList();
        if (_userRepository.UpdateNotificationPreferences(userId, entities).IsError)
        {
            _logger.LogError("Failed to update notification preferences for user {UserId}.", userId);
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
            _logger.LogWarning("Password verification failed: user {UserId} not found.", userId);
            return userResult.FirstError;
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
            _logger.LogWarning("Get sessions failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Session>> sessionsResult = _userRepository.GetActiveSessions(userId);
        if (sessionsResult.IsError)
        {
            _logger.LogError(
                "Failed to fetch sessions for user {UserId}: {Error}",
                userId,
                sessionsResult.FirstError.Description);
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
                CreatedAt = session.CreatedAt,
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
            _logger.LogWarning("Revoke session failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<Success> result = _userRepository.RevokeSession(userId, sessionId);
        if (result.IsError)
        {
            _logger.LogError("Failed to revoke session {SessionId} for user {UserId}.", sessionId, userId);
            return result.FirstError;
        }

        _logger.LogInformation("Session {SessionId} revoked for user {UserId}.", sessionId, userId);
        return Result.Success;
    }

    private static bool IsSupportedOAuthProvider(string provider)
    {
        return string.Equals(provider, GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase);
    }
}
