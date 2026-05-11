namespace BankingApp.Desktop.Tests.ViewModels;

using BankingApp.Application.Features.UserProfile.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

public class SecurityViewModelTests
{
    private readonly Mock<IProfileClientService> _profileClientService;
    private readonly SecurityViewModel _viewModel;

    public SecurityViewModelTests()
    {
        _profileClientService = new Mock<IProfileClientService>();
        _viewModel = new SecurityViewModel(_profileClientService.Object, NullLogger<SecurityViewModel>.Instance);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsFalseWithLengthError()
    {
        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "Short1!", "Short1!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.MinimumLengthRequired, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsFalseWithMismatchError()
    {
        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "Different123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.PasswordMismatch, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenDataIsValid_ReturnsSuccessAndUpdatesState()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPasswordError_UpdatesStateAndReturnsSpecificMessage()
    {
        // Arrange
        var error = Error.Validation("incorrect_password", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.IncorrectPassword, result.ErrorMessage);
        Assert.Equal(ProfileState.Error, _viewModel.State.Value);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsGenericError_UpdatesStateAndReturnsGenericMessage()
    {
        // Arrange
        var error = Error.Failure("server_error", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        (bool Success, string ErrorMessage) result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.UnexpectedError, result.ErrorMessage);
        Assert.Equal(ProfileState.Error, _viewModel.State.Value);
    }

    [Fact]
    public async Task SetTwoFactorEnabled_WhenTrue_CallsEnableTwoFactorAndReturnsTrue()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.Enable2FaAsync(It.IsAny<EnableTwoFaRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await _viewModel.SetTwoFactorEnabled(true);

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task SetTwoFactorEnabled_WhenFalse_CallsDisableTwoFactorAndReturnsTrue()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.Disable2FaAsync())
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await _viewModel.SetTwoFactorEnabled(false);

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task EnableTwoFactor_WhenApiSucceeds_UpdatesStateAndReturnsTrue()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.Enable2FaAsync(It.IsAny<EnableTwoFaRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await _viewModel.EnableTwoFactor(TwoFactorMethod.Email);

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task DisableTwoFactor_WhenApiSucceeds_UpdatesStateAndReturnsTrue()
    {
        // Arrange
        _profileClientService
            .Setup(profileClientService => profileClientService.Disable2FaAsync())
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await _viewModel.DisableTwoFactor();

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task EnableTwoFactor_WhenApiFails_UpdatesStateToErrorAndReturnsFalse()
    {
        // Arrange
        var error = Error.Failure("server_error", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.Enable2FaAsync(It.IsAny<EnableTwoFaRequest>()))
            .ReturnsAsync(error);

        // Act
        bool result = await _viewModel.EnableTwoFactor(TwoFactorMethod.Email);

        // Assert
        Assert.False(result);
        Assert.Equal(ProfileState.Error, _viewModel.State.Value);
    }

    [Fact]
    public async Task DisableTwoFactor_WhenApiFails_UpdatesStateToErrorAndReturnsFalse()
    {
        // Arrange
        var error = Error.Failure("server_error", "Description");
        _profileClientService
            .Setup(profileClientService => profileClientService.Disable2FaAsync())
            .ReturnsAsync(error);

        // Act
        bool result = await _viewModel.DisableTwoFactor();

        // Assert
        Assert.False(result);
        Assert.Equal(ProfileState.Error, _viewModel.State.Value);
    }
}
