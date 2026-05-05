// <copyright file="ForexViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

namespace BankingApp.Desktop.Tests.ViewModels;

using System;
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
///     Tests for the <see cref="ForexViewModel"/>.
/// </summary>
public class ForexViewModelTests
{
    private readonly Mock<IForexRepository> _forexRepository;
    private readonly Mock<IApiClient> _apiClient;
    private readonly ForexViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexViewModelTests"/> class.
    ///     Creates fresh mocks and a view model for each test.
    /// </summary>
    public ForexViewModelTests()
    {
        _forexRepository = new Mock<IForexRepository>(MockBehavior.Loose);
        _apiClient = new Mock<IApiClient>(MockBehavior.Loose);
        _viewModel = new ForexViewModel(
            _forexRepository.Object,
            _apiClient.Object,
            NullLogger<ForexViewModel>.Instance);
    }

    /// <summary>
    ///     LoadPreviewAsync should advance the wizard and populate rates when the repository succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenRepositorySucceeds_AdvancesToStep2AndPopulatesRates()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        var response = new ExchangeTransactionResponseDto
        {
            ExchangeRate = 1.08m,
            Commission = 1.0m,
            TargetAmount = 107.0m,
        };

        _forexRepository
            .Setup(repository => repository.GetRatePreviewAsync("EUR", "USD", 100m))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(2);
        _viewModel.LiveRate.Should().Be(1.08m);
        _viewModel.Commission.Should().Be(1.0m);
        _viewModel.TargetAmount.Should().Be(107.0m);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     LoadPreviewAsync should set an error when the repository fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenRepositoryFails_SetsErrorMessage()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        _forexRepository
            .Setup(repository => repository.GetRatePreviewAsync("EUR", "USD", 100m))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.PreviewFailed);
        _viewModel.CurrentStep.Should().Be(1);
    }

    /// <summary>
    ///     ExecuteExchangeAsync should advance the wizard to the result step when the repository succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteExchangeAsync_WhenRepositorySucceeds_AdvancesToStep4AndSetsReference()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(42);
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        var response = new ExchangeTransactionResponseDto { Id = 12345 };

        _forexRepository
            .Setup(repository => repository.ExecuteExchangeAsync(It.Is<ExchangeTransactionRequestDto>(request =>
                request.UserId == 42 &&
                request.SourceCurrency == "EUR" &&
                request.TargetCurrency == "USD" &&
                request.SourceAmount == 100m)))
            .ReturnsAsync(response);

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.CurrentStep.Should().Be(4);
        _viewModel.TransactionReference.Should().Be("TX-12345");
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     ExecuteExchangeAsync should set an error when the repository fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteExchangeAsync_WhenRepositoryFails_SetsErrorMessage()
    {
        // Arrange
        _apiClient.Setup(client => client.CurrentUserId).Returns(42);
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        _forexRepository
            .Setup(repository => repository.ExecuteExchangeAsync(It.IsAny<ExchangeTransactionRequestDto>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.ExecuteFailed);
        _viewModel.CurrentStep.Should().Be(1);
    }
}
