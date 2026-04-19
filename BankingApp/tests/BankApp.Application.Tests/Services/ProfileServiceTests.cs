// <copyright file="ProfileServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Profile;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using ApplicationTwoFactorMethod = BankApp.Application.Enums.TwoFactorMethod;

namespace BankApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="ProfileService" />.
/// </summary>
public class ProfileServiceTests
{
    private const int NonExistentUserId = 99;
    private readonly Mock<IHashService> _hashService = new(MockBehavior.Strict);
    private readonly ProfileService _service;
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileServiceTests" /> class.
    /// </summary>
    public ProfileServiceTests()
    {
        _service = new ProfileService(
            _userRepository.Object,
            _hashService.Object,
            NullLogger<ProfileService>.Instance);
    }

    /// <summary>
    ///     Verifies the GetProfile_WhenUserExists_ReturnsProfileInfo scenario.
    /// </summary>
    [Fact]
    public void GetProfile_WhenUserExists_ReturnsProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        var dateOfBirth = new DateTime(1815, 12, 10);
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(
                new User
                {
                    Id = userId,
                    FullName = fullName,
                    Email = email,
                    DateOfBirth = dateOfBirth,
                    PreferredLanguage = "ro",
                });

        // Act
        ErrorOr<ProfileInfo> result = _service.GetProfile(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FullName.Should().Be(fullName);
        result.Value.Email.Should().Be(email);
        result.Value.UserId.Should().Be(userId);
        result.Value.DateOfBirth.Should().Be(dateOfBirth);
        result.Value.PreferredLanguage.Should().Be("ro");
    }

    /// <summary>
    ///     Verifies the GetProfile_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetProfile_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<ProfileInfo> result = _service.GetProfile(NonExistentUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the UpdatePersonalInfo_WhenUserIdIsNull_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void UpdatePersonalInfo_WhenUserIdIsNull_ReturnsValidationError()
    {
        // Arrange
        var request = new UpdateProfileRequest(null, "0712345678", "123 Main St");

        // Act
        ErrorOr<Success> result = _service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the UpdatePersonalInfo_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void UpdatePersonalInfo_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());
        var request = new UpdateProfileRequest(NonExistentUserId, "0712345678", "123 Main St");

        // Act
        ErrorOr<Success> result = _service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the UpdatePersonalInfo_WhenPhoneIsInvalid_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void UpdatePersonalInfo_WhenPhoneIsInvalid_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        var request = new UpdateProfileRequest(userId, "not-a-phone", "123 Main St");

        // Act
        ErrorOr<Success> result = _service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("invalid_phone");
    }

    /// <summary>
    ///     Verifies the UpdatePersonalInfo_WhenValid_UpdatesUserAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void UpdatePersonalInfo_WhenValid_UpdatesUserAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string validPhone = "0712345678";
        const string address = "123 Main St";
        const string nationality = "Romanian";
        const string preferredLanguage = "ro";
        var dateOfBirth = new DateTime(1815, 12, 10);
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        _userRepository
            .Setup(updatesUser => updatesUser.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);
        var request = new UpdateProfileRequest(userId, validPhone, address)
        {
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Nationality = nationality,
            PreferredLanguage = preferredLanguage,
        };

        // Act
        ErrorOr<Success> result = _service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(
            updatesUser => updatesUser.UpdateUser(
                It.Is<User>(user =>
                    user.FullName == fullName &&
                    user.PhoneNumber == validPhone &&
                    user.DateOfBirth == dateOfBirth &&
                    user.Address == address &&
                    user.Nationality == nationality &&
                    user.PreferredLanguage == preferredLanguage)),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the LinkOAuth_WhenGoogleIsNotLinked_SavesGoogleLink scenario.
    /// </summary>
    [Fact]
    public void LinkOAuth_WhenGoogleIsNotLinked_SavesGoogleLink()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com" });
        _userRepository
            .Setup(getsLinkedProviders => getsLinkedProviders.GetLinkedProviders(userId))
            .Returns(new List<OAuthLink>());
        _userRepository
            .Setup(savesOAuthLink => savesOAuthLink.SaveOAuthLink(userId, "Google", It.IsAny<string>(), "ada@test.com"))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.LinkOAuth(userId, "Google");

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(
            savesOAuthLink => savesOAuthLink.SaveOAuthLink(userId, "Google", It.IsAny<string>(), "ada@test.com"),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the LinkOAuth_WhenProviderIsUnsupported_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void LinkOAuth_WhenProviderIsUnsupported_ReturnsValidationError()
    {
        // Act
        ErrorOr<Success> result = _service.LinkOAuth(1, "Facebook");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("unsupported_provider");
    }

    /// <summary>
    ///     Verifies the UnlinkOAuth_WhenGoogleIsLinked_DeletesLink scenario.
    /// </summary>
    [Fact]
    public void UnlinkOAuth_WhenGoogleIsLinked_DeletesLink()
    {
        // Arrange
        const int userId = 1;
        const int linkId = 7;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId });
        _userRepository
            .Setup(getsLinkedProviders => getsLinkedProviders.GetLinkedProviders(userId))
            .Returns(new List<OAuthLink> { new() { Id = linkId, Provider = "Google" } });
        _userRepository
            .Setup(deletesOAuthLink => deletesOAuthLink.DeleteOAuthLink(linkId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.UnlinkOAuth(userId, "Google");

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(deletesOAuthLink => deletesOAuthLink.DeleteOAuthLink(linkId), Times.Once);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void ChangePassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());
        var request = new ChangePasswordRequest(NonExistentUserId, "old", "NewPass1!");

        // Act
        ErrorOr<Success> result = _service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenNewPasswordIsWeak_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void ChangePassword_WhenNewPasswordIsWeak_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" });
        var request = new ChangePasswordRequest(userId, "old", "weak");

        // Act
        ErrorOr<Success> result = _service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("weak_password");
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenCurrentPasswordIsWrong_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void ChangePassword_WhenCurrentPasswordIsWrong_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" });
        _hashService
            .Setup(verifies => verifies.Verify("wrongpassword", "hash"))
            .Returns(false);
        var request = new ChangePasswordRequest(userId, "wrongpassword", "NewPass1!");

        // Act
        ErrorOr<Success> result = _service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("incorrect_password");
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenValid_UpdatesPasswordAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void ChangePassword_WhenValid_UpdatesPasswordAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const string newPassword = "NewPass1!";
        const string newHash = "newhash";
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "oldhash" });
        _hashService
            .Setup(verifies => verifies.Verify("oldpassword", "oldhash"))
            .Returns(true);
        _hashService
            .Setup(getsHash => getsHash.GetHash(newPassword))
            .Returns((ErrorOr<string>)newHash);
        _userRepository
            .Setup(updatesPassword => updatesPassword.UpdatePassword(userId, newHash))
            .Returns(Result.Success);
        var request = new ChangePasswordRequest(userId, "oldpassword", newPassword);

        // Act
        ErrorOr<Success> result = _service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(updatesPassword => updatesPassword.UpdatePassword(userId, newHash), Times.Once);
    }

    /// <summary>
    ///     Verifies the Enable2FA_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void Enable2FA_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = _service.Enable2Fa(NonExistentUserId, ApplicationTwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the Enable2FA_WhenUserExists_EnablesTwoFactorAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void Enable2FA_WhenUserExists_EnablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        _userRepository
            .Setup(updatesUser => updatesUser.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Enable2Fa(userId, ApplicationTwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(
            updatesUser => updatesUser.UpdateUser(
                It.Is<User>(user =>
                    user.Is2FaEnabled && user.Preferred2FaMethod == TwoFactorMethod.Email)),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the Disable2FA_WhenUserExists_DisablesTwoFactorAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void Disable2FA_WhenUserExists_DisablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", Is2FaEnabled = true });
        _userRepository
            .Setup(updatesUser => updatesUser.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Disable2Fa(userId);

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(
            updatesUser => updatesUser.UpdateUser(
                It.Is<User>(user =>
                    !user.Is2FaEnabled && user.Preferred2FaMethod == null)),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the VerifyPassword_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void VerifyPassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<bool> result = _service.VerifyPassword(NonExistentUserId, "anypassword");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the VerifyPassword_WhenPasswordMatches_ReturnsTrue scenario.
    /// </summary>
    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ReturnsTrue()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "correcthash" });
        _hashService
            .Setup(verifies => verifies.Verify("correctpassword", "correcthash"))
            .Returns(true);

        // Act
        ErrorOr<bool> result = _service.VerifyPassword(userId, "correctpassword");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "correcthash" });
        _hashService
            .Setup(verifies => verifies.Verify("wrongpassword", "correcthash"))
            .Returns(false);

        // Act
        ErrorOr<bool> result = _service.VerifyPassword(userId, "wrongpassword");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the GetNotificationPreferences_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetNotificationPreferences_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<List<NotificationPreferenceDataTransferObject>> result =
            _service.GetNotificationPreferences(NonExistentUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetNotificationPreferences_WhenUserExists_ReturnsMappedPreferences scenario.
    /// </summary>
    [Fact]
    public void GetNotificationPreferences_WhenUserExists_ReturnsMappedPreferences()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        _userRepository
            .Setup(getsNotificationPreferences => getsNotificationPreferences.GetNotificationPreferences(userId))
            .Returns(
                new List<NotificationPreference>
                {
                    new() { Id = 1, UserId = userId, Category = NotificationType.Payment, EmailEnabled = true },
                });

        // Act
        ErrorOr<List<NotificationPreferenceDataTransferObject>> result = _service.GetNotificationPreferences(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().EmailEnabled.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the GetActiveSessions_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetActiveSessions_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<List<SessionDataTransferObject>> result = _service.GetActiveSessions(NonExistentUserId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetActiveSessions_WhenUserExists_ReturnsMappedSessionDtos scenario.
    /// </summary>
    [Fact]
    public void GetActiveSessions_WhenUserExists_ReturnsMappedSessionDtos()
    {
        // Arrange
        const int userId = 1;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        _userRepository
            .Setup(getsActiveSessions => getsActiveSessions.GetActiveSessions(userId))
            .Returns(
                new List<Session>
                {
                    new() { Id = 1, UserId = userId, Token = "token1", DeviceInfo = "Chrome/Windows" },
                });

        // Act
        ErrorOr<List<SessionDataTransferObject>> result = _service.GetActiveSessions(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().Id.Should().Be(1);
        result.Value.First().DeviceInfo.Should().Be("Chrome/Windows");
    }

    /// <summary>
    ///     Verifies the RevokeSession_WhenUserDoesNotExist_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void RevokeSession_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository
            .Setup(findsById => findsById.FindById(NonExistentUserId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = _service.RevokeSession(NonExistentUserId, 1);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the RevokeSession_WhenUserExists_RevokesSessionAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void RevokeSession_WhenUserExists_RevokesSessionAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const int sessionId = 42;
        _userRepository
            .Setup(findsById => findsById.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        _userRepository
            .Setup(revokesSession => revokesSession.RevokeSession(userId, sessionId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.RevokeSession(userId, sessionId);

        // Assert
        result.IsError.Should().BeFalse();
        _userRepository.Verify(revokesSession => revokesSession.RevokeSession(userId, sessionId), Times.Once);
    }
}
