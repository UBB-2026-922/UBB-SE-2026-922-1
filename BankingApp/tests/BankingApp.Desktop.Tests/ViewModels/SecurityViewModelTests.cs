// <copyright file="SecurityViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BankingApp.Desktop.Tests.ViewModels;

public class SecurityViewModelTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<ILogger<SecurityViewModel>> _mockLogger;
    private readonly SecurityViewModel _viewModel;

    public SecurityViewModelTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _mockLogger = new Mock<ILogger<SecurityViewModel>>();
        _viewModel = new SecurityViewModel(_mockApiClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsFalseWithLengthError()
    {
        // Act
        var result = await _viewModel.ChangePassword(1, "OldPass123!", "Short1!", "Short1!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.MinimumLengthRequired, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsFalseWithMismatchError()
    {
        // Act
        var result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "Different123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.PasswordMismatch, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_WhenDataIsValid_ReturnsSuccessAndUpdatesState()
    {
        // Arrange
        _mockApiClient
            .Setup(c => c.PutAsync(ApiEndpoints.ChangePassword, It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        var result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ProfileState.UpdateSuccess, _viewModel.State.Value);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPasswordError_UpdatesStateAndReturnsSpecificMessage()
    {
        // Arrange
        var error = Error.Validation("incorrect_password", "Description");
        _mockApiClient
            .Setup(c => c.PutAsync(ApiEndpoints.ChangePassword, It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        var result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

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
        _mockApiClient
            .Setup(c => c.PutAsync(ApiEndpoints.ChangePassword, It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(error);

        // Act
        var result = await _viewModel.ChangePassword(1, "OldPass123!", "NewPass123!", "NewPass123!");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(UserMessages.Security.UnexpectedError, result.ErrorMessage);
        Assert.Equal(ProfileState.Error, _viewModel.State.Value);
    }
}
