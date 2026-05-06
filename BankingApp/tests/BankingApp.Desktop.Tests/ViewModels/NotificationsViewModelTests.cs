// <copyright file="NotificationsViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Application.Enums;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

public class NotificationsViewModelTests
{
    private readonly Mock<IApiClient> _apiClient = new(MockBehavior.Strict);

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreferenceAndSetsSuccessState()
    {
        // Arrange
        var preference = new NotificationPreferenceDataTransferObject
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = false,
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(
                ApiEndpoints.NotificationPreferences,
                viewModel.NotificationPreferences))
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
        var preference = new NotificationPreferenceDataTransferObject
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(preference);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(
                ApiEndpoints.NotificationPreferences,
                viewModel.NotificationPreferences))
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
        var preferences = new List<NotificationPreferenceDataTransferObject>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = true },
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);

        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<List<NotificationPreferenceDataTransferObject>>(
                ApiEndpoints.NotificationPreferences,
                It.IsAny<CancellationToken>()))
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
        var existingPreference = new NotificationPreferenceDataTransferObject
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _apiClient
            .Setup(getsAsync => getsAsync.GetAsync<List<NotificationPreferenceDataTransferObject>>(
                ApiEndpoints.NotificationPreferences,
                It.IsAny<CancellationToken>()))
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
        var updatedPreferences = new List<NotificationPreferenceDataTransferObject>
        {
            new() { Id = 1, Category = NotificationType.Payment, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(ApiEndpoints.NotificationPreferences, updatedPreferences))
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
        var existingPreference = new NotificationPreferenceDataTransferObject
        {
            Id = 1,
            Category = NotificationType.Payment,
            EmailEnabled = true,
        };
        var updatedPreferences = new List<NotificationPreferenceDataTransferObject>
        {
            new() { Id = 2, Category = NotificationType.LowBalance, EmailEnabled = false },
        };

        var viewModel = new NotificationsViewModel(_apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        viewModel.NotificationPreferences.Add(existingPreference);

        _apiClient
            .Setup(putsAsync => putsAsync.PutAsync(ApiEndpoints.NotificationPreferences, updatedPreferences))
            .ReturnsAsync(Error.Failure(description: "save failed"));

        // Act
        bool success = await viewModel.UpdateNotificationPreferences(updatedPreferences);

        // Assert
        success.Should().BeFalse();
        viewModel.NotificationPreferences.Should().ContainSingle().Which.Should().BeSameAs(existingPreference);
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }
}
