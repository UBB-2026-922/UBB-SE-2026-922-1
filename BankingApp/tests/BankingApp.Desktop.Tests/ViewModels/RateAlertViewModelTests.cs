// <copyright file="RateAlertViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

namespace BankingApp.Desktop.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.Repositories;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FluentAssertions;
using Xunit;

/// <summary>
///     Tests for the <see cref="RateAlertViewModel"/>.
/// </summary>
public class RateAlertViewModelTests
{
    private readonly Mock<IRateAlertRepository> _rateAlertRepository;
    private readonly Mock<IApiClient> _apiClient;
    private readonly RateAlertViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertViewModelTests"/> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public RateAlertViewModelTests()
    {
        _rateAlertRepository = new Mock<IRateAlertRepository>(MockBehavior.Loose);
        _apiClient = new Mock<IApiClient>(MockBehavior.Loose);
        _viewModel = new RateAlertViewModel(
            _rateAlertRepository.Object,
            _apiClient.Object,
            NullLogger<RateAlertViewModel>.Instance);
    }

    /// <summary>
    ///     LoadAlertsAsync should populate Alerts when the repository succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAlertsAsync_WhenRepositorySucceeds_PopulatesAlerts()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);
        var alerts = new List<RateAlertDto>
        {
            new RateAlertDto { Id = 1, BaseCurrency = "EUR", TargetCurrency = "USD", TargetRate = 1.10m },
            new RateAlertDto { Id = 2, BaseCurrency = "GBP", TargetCurrency = "RON", TargetRate = 5.80m },
        };

        _rateAlertRepository
            .Setup(repository => repository.GetAlertsAsync(It.IsAny<int>()))
            .ReturnsAsync(alerts);

        // Act
        await _viewModel.LoadAlertsAsync();

        // Assert
        _viewModel.Alerts.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     LoadAlertsAsync should set error when the repository fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadAlertsAsync_WhenRepositoryFails_SetsError()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);
        _rateAlertRepository
            .Setup(repository => repository.GetAlertsAsync(It.IsAny<int>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadAlertsAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.LoadFailed);
    }

    /// <summary>
    ///     CreateAlertAsync should set error when currencies are empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenCurrenciesAreEmpty_SetsError()
    {
        // Arrange
        _viewModel.TargetRateText = "1.5";

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.CurrencyRequired);
    }

    /// <summary>
    ///     CreateAlertAsync should set error when currencies are the same.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenCurrenciesAreEqual_SetsError()
    {
        // Arrange
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "EUR";
        _viewModel.TargetRateText = "1.5";

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.CurrenciesMustDiffer);
    }

    /// <summary>
    ///     CreateAlertAsync should set error when target rate text has ambiguous format.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenRateFormatIsAmbiguous_SetsError()
    {
        // Arrange
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.TargetRateText = "1,234.56";

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.InvalidNumberFormat);
    }

    /// <summary>
    ///     CreateAlertAsync should set error when target rate is not a valid number.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenRateIsInvalid_SetsError()
    {
        // Arrange
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.TargetRateText = "abc";

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.InvalidTargetRate);
    }

    /// <summary>
    ///     CreateAlertAsync should add the new alert and clear inputs on success.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenRepositorySucceeds_AddsAlertAndClearsInputs()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.TargetRateText = "1.10";

        var created = new RateAlertDto
        {
            Id = 42,
            BaseCurrency = "EUR",
            TargetCurrency = "USD",
            TargetRate = 1.10m,
        };

        _rateAlertRepository
            .Setup(repository => repository.CreateAlertAsync(It.IsAny<RateAlertDto>()))
            .ReturnsAsync(created);

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.Alerts.Should().ContainSingle().Which.Id.Should().Be(42);
        _viewModel.BaseCurrency.Should().BeEmpty();
        _viewModel.TargetCurrency.Should().BeEmpty();
        _viewModel.TargetRateText.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     CreateAlertAsync should set error when the repository fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CreateAlertAsync_WhenRepositoryFails_SetsError()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.TargetRateText = "1.10";

        _rateAlertRepository
            .Setup(repository => repository.CreateAlertAsync(It.IsAny<RateAlertDto>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.CreateFailed);
    }

    /// <summary>
    ///     DeleteAlertAsync should remove the alert from the collection on success.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAlertAsync_WhenRepositorySucceeds_RemovesAlert()
    {
        // Arrange
        const int alertId = 1;
        _viewModel.Alerts.Add(new RateAlertDto { Id = alertId, BaseCurrency = "EUR", TargetCurrency = "USD" });

        _rateAlertRepository
            .Setup(repository => repository.DeleteAlertAsync(alertId))
            .ReturnsAsync(Result.Success);

        // Act
        await _viewModel.DeleteAlertAsync(alertId);

        // Assert
        _viewModel.Alerts.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     DeleteAlertAsync should set error when the repository fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAlertAsync_WhenRepositoryFails_SetsError()
    {
        // Arrange
        const int alertId = 1;
        _viewModel.Alerts.Add(new RateAlertDto { Id = alertId, BaseCurrency = "EUR", TargetCurrency = "USD" });

        _rateAlertRepository
            .Setup(repository => repository.DeleteAlertAsync(alertId))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.DeleteAlertAsync(alertId);

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.DeleteFailed);
        _viewModel.Alerts.Should().HaveCount(1);
    }
}
