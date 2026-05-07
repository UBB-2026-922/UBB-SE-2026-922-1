using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

public class SessionsViewModelTests
{
    private readonly Mock<IProfileClientService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task LoadSessionsAsync_WhenApiReturnsSessions_PopulatesCollectionAndResetsState()
    {
        // Arrange
        const int userId = 7;
        var sessions = new List<SessionDto>
        {
            new() { Id = 1, DeviceInfo = "Desktop" },
            new() { Id = 2, DeviceInfo = "Phone" },
        };

        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.GetSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.Idle);
        viewModel.ActiveSessions.Should().BeSameAs(sessions);
    }

    [Fact]
    public async Task LoadSessionsAsync_WhenApiReturnsError_ClearsCollectionAndSetsErrorState()
    {
        // Arrange
        const int userId = 7;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.GetSessionsAsync())
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
        viewModel.ActiveSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadSessionsAsync_WhenApiThrows_ClearsCollectionAndSetsErrorState()
    {
        // Arrange
        const int userId = 7;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.GetSessionsAsync())
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        bool success = await viewModel.LoadSessionsAsync(userId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
        viewModel.ActiveSessions.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiSucceeds_ReturnsTrueAndResetsState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.RevokeSessionAsync(sessionId))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiReturnsError_SetsErrorState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.RevokeSessionAsync(sessionId))
            .ReturnsAsync(Error.Failure(description: "revoke failed"));

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenApiThrows_SetsErrorState()
    {
        // Arrange
        const int sessionId = 42;
        var viewModel = new SessionsViewModel(_profileClientService.Object, NullLogger<SessionsViewModel>.Instance);

        _profileClientService
            .Setup(s => s.RevokeSessionAsync(sessionId))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        bool success = await viewModel.RevokeSessionAsync(sessionId);

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }
}
