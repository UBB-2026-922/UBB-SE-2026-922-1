using BankingApp.Application.DTOs.RateAlerts;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

public class RateAlertViewModelTests
{
    private readonly Mock<IRateAlertClientService> _rateAlertClientService;
    private readonly RateAlertViewModel _viewModel;

    public RateAlertViewModelTests()
    {
        _rateAlertClientService = new Mock<IRateAlertClientService>(MockBehavior.Loose);
        _viewModel = new RateAlertViewModel(_rateAlertClientService.Object, NullLogger<RateAlertViewModel>.Instance);
    }

    [Fact]
    public async Task LoadAlertsAsync_WhenApiSucceeds_PopulatesAlerts()
    {
        // Arrange
        _rateAlertClientService.Setup(s => s.CurrentUserId).Returns(1);
        var alerts = new List<RateAlertDto>
        {
            new RateAlertDto { Id = 1, BaseCurrency = "EUR", TargetCurrency = "USD", TargetRate = 1.10m },
            new RateAlertDto { Id = 2, BaseCurrency = "GBP", TargetCurrency = "RON", TargetRate = 5.80m },
        };

        _rateAlertClientService
            .Setup(s => s.GetAlertsAsync(It.IsAny<int>()))
            .ReturnsAsync(alerts);

        // Act
        await _viewModel.LoadAlertsAsync();

        // Assert
        _viewModel.Alerts.Should().HaveCount(2);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAlertsAsync_WhenApiFails_SetsError()
    {
        // Arrange
        _rateAlertClientService.Setup(s => s.CurrentUserId).Returns(1);
        _rateAlertClientService
            .Setup(s => s.GetAlertsAsync(It.IsAny<int>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.LoadAlertsAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.LoadFailed);
    }

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

    [Fact]
    public async Task CreateAlertAsync_WhenApiSucceeds_AddsAlertAndClearsInputs()
    {
        // Arrange
        _rateAlertClientService.Setup(s => s.CurrentUserId).Returns(1);
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

        _rateAlertClientService
            .Setup(s => s.CreateAlertAsync(It.IsAny<RateAlertDto>()))
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

    [Fact]
    public async Task CreateAlertAsync_WhenApiFails_SetsError()
    {
        // Arrange
        _rateAlertClientService.Setup(s => s.CurrentUserId).Returns(1);
        _viewModel.BaseCurrency = "EUR";
        _viewModel.TargetCurrency = "USD";
        _viewModel.TargetRateText = "1.10";

        _rateAlertClientService
            .Setup(s => s.CreateAlertAsync(It.IsAny<RateAlertDto>()))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.CreateAlertAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.CreateFailed);
    }

    [Fact]
    public async Task DeleteAlertAsync_WhenApiSucceeds_RemovesAlert()
    {
        // Arrange
        const int alertId = 1;
        _viewModel.Alerts.Add(new RateAlertDto { Id = alertId, BaseCurrency = "EUR", TargetCurrency = "USD" });

        _rateAlertClientService
            .Setup(s => s.DeleteAlertAsync(alertId))
            .ReturnsAsync(Result.Success);

        // Act
        await _viewModel.DeleteAlertAsync(alertId);

        // Assert
        _viewModel.Alerts.Should().BeEmpty();
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAlertAsync_WhenApiFails_SetsError()
    {
        // Arrange
        const int alertId = 1;
        _viewModel.Alerts.Add(new RateAlertDto { Id = alertId, BaseCurrency = "EUR", TargetCurrency = "USD" });

        _rateAlertClientService
            .Setup(s => s.DeleteAlertAsync(alertId))
            .ReturnsAsync(Error.Failure());

        // Act
        await _viewModel.DeleteAlertAsync(alertId);

        // Assert
        _viewModel.ErrorMessage.Should().Be(UserMessages.RateAlerts.DeleteFailed);
        _viewModel.Alerts.Should().HaveCount(1);
    }
}
