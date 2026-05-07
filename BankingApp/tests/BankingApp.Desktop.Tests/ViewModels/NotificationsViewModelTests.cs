namespace BankingApp.Desktop.Tests.ViewModels;

using Application.DTOs.Profile;
using Enums;
using Services;
using BankingApp.Desktop.ViewModels;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class NotificationsViewModelTests
{
    private readonly Mock<IProfileClientService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreferenceAndSetsSuccessState()
    {
        // Arrange
        var preference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = false,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _profileClientService
            .Setup(service => service.UpdateNotificationPreferencesAsync(viewModel.NotificationPreferences))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.ToggleNotificationPreference(preference, true);

        // Assert
        success.Should().BeTrue();
        preference.EmailEnabled.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_RollsBackPreferenceAndSetsErrorState()
    {
        // Arrange
        var preference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _profileClientService
            .Setup(s => s.UpdateNotificationPreferencesAsync(viewModel.NotificationPreferences))
            .ReturnsAsync(Error.Failure(description: "save failed"));

        // Act
        bool success = await viewModel.ToggleNotificationPreference(preference, false);

        // Assert
        success.Should().BeFalse();
        preference.EmailEnabled.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task LoadNotificationPreferences_WhenApiReturnsPreferences_PopulatesCollection()
    {
        // Arrange
        var preferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = true },
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.GetNotificationPreferencesAsync())
            .ReturnsAsync(preferences);

        // Act
        bool success = await viewModel.LoadNotificationPreferences();

        // Assert
        success.Should().BeTrue();
        viewModel.NotificationPreferences.Should().BeSameAs(preferences);
        viewModel.State.Value.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task LoadNotificationPreferences_WhenApiFails_PreservesExistingPreferences()
    {
        // Arrange
        var existingPreference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _profileClientService
            .Setup(s => s.GetNotificationPreferencesAsync())
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadNotificationPreferences();

        // Assert
        success.Should().BeFalse();
        viewModel.NotificationPreferences.Should().ContainSingle().Which.Should().BeSameAs(existingPreference);
        viewModel.State.Value.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenApiSucceeds_ReplacesPreferencesAndSetsSuccessState()
    {
        // Arrange
        var updatedPreferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);

        _profileClientService
            .Setup(service => service.UpdateNotificationPreferencesAsync(updatedPreferences))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdateNotificationPreferences(updatedPreferences);

        // Assert
        success.Should().BeTrue();
        viewModel.NotificationPreferences.Should().BeSameAs(updatedPreferences);
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenApiFails_PreservesExistingPreferencesAndSetsErrorState()
    {
        // Arrange
        var existingPreference = new NotificationPreferenceDto
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };
        var updatedPreferences = new List<NotificationPreferenceDto>
        {
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_profileClientService.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _profileClientService
            .Setup(s => s.UpdateNotificationPreferencesAsync(updatedPreferences))
            .ReturnsAsync(Error.Failure(description: "save failed"));

        // Act
        bool success = await viewModel.UpdateNotificationPreferences(updatedPreferences);

        // Assert
        success.Should().BeFalse();
        viewModel.NotificationPreferences.Should().ContainSingle().Which.Should().BeSameAs(existingPreference);
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }
}
