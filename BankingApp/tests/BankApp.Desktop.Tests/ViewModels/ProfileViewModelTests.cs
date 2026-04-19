// <copyright file="ProfileViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for the profile sub-ViewModels: <see cref="ProfileViewModel" />,
///     <see cref="PersonalInfoViewModel" />, <see cref="SecurityViewModel" />,
///     <see cref="OAuthViewModel" />, and <see cref="NotificationsViewModel" />.
/// </summary>
public class ProfileViewModelTests
{
    private readonly Mock<IApiClient> _apiClient = new(MockBehavior.Strict);

    /// <summary>
    ///     Verifies the LoadProfile_WhenApiReturnsProfile_PopulatesProfileInfo scenario.
    /// </summary>
    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string email = "test@bank.com";
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var viewModel = new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ProfileInfo
                {
                    UserId = userId,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                });

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
        viewModel.ProfileInfo.FullName.Should().Be(fullName);
        viewModel.ProfileInfo.Email.Should().Be(email);
    }

    /// <summary>
    ///     Verifies the LoadProfile_WhenApiFails_SetsErrorState scenario.
    /// </summary>
    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    /// <summary>
    ///     Verifies the HasPhoneNumber_WhenPhoneNumberIsNotSet_ReturnsFalseAndShowsPlaceholder scenario.
    /// </summary>
    [Fact]
    public void HasPhoneNumber_WhenPhoneNumberIsNotSet_ReturnsFalseAndShowsPlaceholder()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Assert
        viewModel.HasPhoneNumber.Should().BeFalse();
        viewModel.TwoFactorPhoneDisplay.Should().Be(UserMessages.Profile.NoPhoneNumber);
    }

    /// <summary>
    ///     Verifies the UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState scenario.
    /// </summary>
    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        const string phoneNumber = "0712345678";
        const string address = "123 Main St";
        const string password = "password";
        var viewModel = new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<UpdateProfileRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdatePersonalInfo(phoneNumber, address, password);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenPasswordTooShort_ReturnsError scenario.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string shortPassword = "short";
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            shortPassword,
            shortPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.MinimumLengthRequired);
        viewModel.State.Value.Should().Be(ProfileState.Idle);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenPasswordsDoNotMatch_ReturnsError scenario.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string newPassword = "LongEnough1!";
        const string confirmPassword = "Different1!";
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            newPassword,
            confirmPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.PasswordMismatch);
        viewModel.State.Value.Should().Be(ProfileState.Idle);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenValid_SetsUpdateSuccessState scenario.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WhenValid_SetsUpdateSuccessState()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string validPassword = "ValidPass1!";
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        (bool success, string error) = await viewModel.ChangePassword(
            userId,
            currentPassword,
            validPassword,
            validPassword);

        // Assert
        success.Should().BeTrue();
        error.Should().BeEmpty();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    /// <summary>
    ///     Verifies the ChangePassword_WhenApiReturnsIncorrectPassword_ReturnsSpecificMessage scenario.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPassword_ReturnsSpecificMessage()
    {
        // Arrange
        const int userId = 1;
        const string wrongPassword = "wrong";
        const string validPassword = "ValidPass1!";
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Error.Validation("incorrect_password", "Wrong password"));

        // Act
        (bool success, string error) =
            await viewModel.ChangePassword(userId, wrongPassword, validPassword, validPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.IncorrectPassword);
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    /// <summary>
    ///     Verifies the SetTwoFactorEnabled_WhenApiSucceeds_ReturnsTrue scenario.
    /// </summary>
    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await viewModel.SetTwoFactorEnabled(true);

        // Assert
        result.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    /// <summary>
    ///     Verifies the DisableTwoFactor_WhenApiSucceeds_ReturnsTrue scenario.
    /// </summary>
    [Fact]
    public async Task DisableTwoFactor_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await viewModel.SetTwoFactorEnabled(false);

        // Assert
        result.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    /// <summary>
    ///     Verifies the SetTwoFactorEnabled_WhenApiFails_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiFails_ReturnsFalse()
    {
        // Arrange
        var viewModel = new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool result = await viewModel.SetTwoFactorEnabled(true);

        // Assert
        result.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    /// <summary>
    ///     Verifies the ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreference scenario.
    /// </summary>
    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        var notificationPreference = new NotificationPreferenceDataTransferObject
            { Id = preferenceId, EmailEnabled = false };
        viewModel.NotificationPreferences.Add(notificationPreference);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(
                It.IsAny<string>(),
                It.IsAny<List<NotificationPreferenceDataTransferObject>>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.ToggleNotificationPreference(notificationPreference, true);

        // Assert
        success.Should().BeTrue();
        notificationPreference.EmailEnabled.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    /// <summary>
    ///     Verifies the ToggleNotificationPreference_WhenApiFails_RollsBackPreference scenario.
    /// </summary>
    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_RollsBackPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        var notificationPreference = new NotificationPreferenceDataTransferObject
            { Id = preferenceId, EmailEnabled = true };
        viewModel.NotificationPreferences.Add(notificationPreference);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(
                It.IsAny<string>(),
                It.IsAny<List<NotificationPreferenceDataTransferObject>>()))
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool success = await viewModel.ToggleNotificationPreference(notificationPreference, false);

        // Assert
        success.Should().BeFalse();
        notificationPreference.EmailEnabled.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the UpdateNotificationPreferences_WhenListIsEmpty_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public async Task UpdateNotificationPreferences_WhenListIsEmpty_ReturnsFalse()
    {
        // Arrange
        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);

        // Act
        bool result =
            await viewModel.UpdateNotificationPreferences(new List<NotificationPreferenceDataTransferObject>());

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the UnlinkOAuth_WhenProviderExists_RemovesAndReturnsTrue scenario.
    /// </summary>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderExists_RemovesAndReturnsTrue()
    {
        // Arrange
        const string provider = "Google";
        const string providerEmail = "user@gmail.com";
        var viewModel = new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance);
        viewModel.OAuthLinks.Add(
            new OAuthLinkDataTransferObject { Provider = provider, ProviderEmail = providerEmail });
        _apiClient
            .Setup(deletesAsync => deletesAsync.DeleteAsync($"{ApiEndpoints.UnlinkOAuth}/{provider}"))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await viewModel.UnlinkOAuth(provider);

        // Assert
        result.Should().BeTrue();
        viewModel.OAuthLinks.Should().BeEmpty();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    /// <summary>
    ///     Verifies the UnlinkOAuth_WhenProviderDoesNotExist_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        const string provider = "Facebook";
        var viewModel = new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance);

        // Act
        bool result = await viewModel.UnlinkOAuth(provider);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the UnlinkOAuth_WhenProviderIsNullOrWhitespace_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderIsNullOrWhitespace_ReturnsFalse()
    {
        // Arrange
        var viewModel = new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance);

        // Assert
        (await viewModel.UnlinkOAuth(string.Empty)).Should().BeFalse();
        (await viewModel.UnlinkOAuth("  ")).Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the LinkOAuth_WhenAlreadyLinked_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public async Task LinkOAuth_WhenAlreadyLinked_ReturnsFalse()
    {
        // Arrange
        const string provider = "Google";
        var viewModel = new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance);
        viewModel.OAuthLinks.Add(new OAuthLinkDataTransferObject { Provider = provider });

        // Act
        bool result = await viewModel.LinkOAuth(provider);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the LoadProfile_WhenPersonalInfoFails_SetsErrorState scenario.
    /// </summary>
    [Fact]
    public async Task LoadProfile_WhenPersonalInfoFails_SetsErrorState()
    {
        // Arrange
        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(description: "fail"));

        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(_apiClient.Object, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Act
        bool success = await profileVm.LoadProfile();

        // Assert
        success.Should().BeFalse();
        profileVm.State.Value.Should().Be(ProfileState.Error);
    }

    /// <summary>
    ///     Verifies the IsInitializingView_DefaultsFalse_CanBeToggled scenario.
    /// </summary>
    [Fact]
    public void IsInitializingView_DefaultsFalse_CanBeToggled()
    {
        // Arrange
        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(_apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(_apiClient.Object, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(_apiClient.Object, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(_apiClient.Object, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Assert initial state
        profileVm.IsInitializingView.Should().BeFalse();

        // Act
        profileVm.IsInitializingView = true;

        // Assert toggled state
        profileVm.IsInitializingView.Should().BeTrue();
    }
}