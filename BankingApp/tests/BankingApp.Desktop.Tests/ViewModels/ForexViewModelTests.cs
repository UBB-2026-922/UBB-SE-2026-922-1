// <copyright file="FXViewModelTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.DTOs.Exchange;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

/// <summary>
///     Tests for the <see cref="ForexViewModel"/>.
/// </summary>
public class ForexViewModelTests
{
    private readonly Mock<IApiClient> _apiClient;
    private readonly ForexViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexViewModelTests"/> class.
    ///     Creates a fresh mock and view model for each test.
    /// </summary>
    public ForexViewModelTests()
    {
        _apiClient = new Mock<IApiClient>(MockBehavior.Loose);
        _viewModel = new ForexViewModel(_apiClient.Object, NullLogger<ForexViewModel>.Instance);
    }

    /// <summary>
    ///     LoadPreviewAsync should set an error when source currency is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenSourceCurrencyIsEmpty_SetsError()
    {
        // Arrange
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.CurrencyRequired);
    }

    /// <summary>
    ///     LoadPreviewAsync should set an error when target currency is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenTargetCurrencyIsEmpty_SetsError()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.AmountText = "100";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.CurrencyRequired);
    }

    /// <summary>
    ///     LoadPreviewAsync should set an error when amount is zero.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenAmountIsZero_SetsError()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "0";

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.AmountRequired);
    }

    /// <summary>
    ///     LoadPreviewAsync should populate rate data when the API succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenApiSucceeds_PopulatesRateData()
    {
        // Arrange
        const decimal expectedRate = 1.12m;
        const decimal expectedCommission = 0.50m;
        const decimal expectedTargetAmount = 111.50m;

        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        var response = new ExchangeTransactionResponseDto
        {
            ExchangeRate = expectedRate,
            Commission = expectedCommission,
            TargetAmount = expectedTargetAmount
        };

        _apiClient
            .Setup(client => client.GetAsync<ExchangeTransactionResponseDto>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.LiveRate.Should().Be(expectedRate);
        _viewModel.Commission.Should().Be(expectedCommission);
        _viewModel.TargetAmount.Should().Be(expectedTargetAmount);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     LoadPreviewAsync should set error message when the API returns an error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadPreviewAsync_WhenApiFails_SetsErrorMessage()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        _apiClient
            .Setup(client => client.GetAsync<ExchangeTransactionResponseDto>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadPreviewAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.PreviewFailed);
    }

    /// <summary>
    ///     ExecuteExchangeAsync should set error when amount is zero.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteExchangeAsync_WhenAmountIsZero_SetsError()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "0";

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.AmountRequired);
    }

    /// <summary>
    ///     ExecuteExchangeAsync should set transaction reference on success.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteExchangeAsync_WhenApiSucceeds_SetsTransactionReference()
    {
        // Arrange
        const int transactionId = 42;
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);

        var response = new ExchangeTransactionResponseDto { Id = transactionId };
        _apiClient
            .Setup(client => client.PostAsync<ExchangeTransactionRequestDto, ExchangeTransactionResponseDto>(
                It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.TransactionReference.Should().Be($"TX-{transactionId}");
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    /// <summary>
    ///     ExecuteExchangeAsync should set error message when the API returns an error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteExchangeAsync_WhenApiFails_SetsErrorMessage()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";
        _apiClient.Setup(client => client.CurrentUserId).Returns(1);

        _apiClient
            .Setup(client => client.PostAsync<ExchangeTransactionRequestDto, ExchangeTransactionResponseDto>(
                It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.ExecuteExchangeAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.Exchange.ExecuteFailed);
    }

    /// <summary>
    ///     Reset should clear all state and return to step 1.
    /// </summary>
    [Fact]
    public void Reset_ClearsAllState()
    {
        // Arrange
        _viewModel.SourceCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.AmountText = "100";

        // Act
        _viewModel.Reset();

        // Assert
        _viewModel.SourceCurrency.Should().BeEmpty();
        _viewModel.TargetCurrency.Should().BeEmpty();
        _viewModel.AmountText.Should().BeEmpty();
        _viewModel.LiveRate.Should().Be(0);
        _viewModel.Commission.Should().Be(0);
        _viewModel.TargetAmount.Should().Be(0);
        _viewModel.TransactionReference.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.CurrentStep.Should().Be(1);
    }
}
