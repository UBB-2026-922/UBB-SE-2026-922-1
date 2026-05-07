namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Application.Features.ForexRateAlerts.Dtos;

using BankingApp.Application.Features.Forex.Services;
using BankingApp.Application.Features.ForexRateAlerts.Services;
using Domain.Entities;
using ErrorOr;

/// <summary>
///     Unit tests for <see cref="ForexRateAlertService" />.
/// </summary>
public class ForexRateAlertServiceTests
{
    private const int ValidUserId = 1;
    private const int ValidAlertId = 10;
    private const int NonExistentAlertId = 99;
    private const string EurCurrency = "EUR";
    private const string UsdCurrency = "USD";
    private const decimal ValidTargetRate = 1.20m;
    private const decimal EurUsdRate = 1.15m;
    private const decimal BuyAlertTargetRateAboveCurrent = 1.20m;
    private const decimal SellAlertTargetRateBelowCurrent = 1.10m;

    private readonly Mock<IForexRateAlertRepository> _rateAlertRepository = new(MockBehavior.Strict);
    private readonly Mock<IForexService> _exchangeService = new(MockBehavior.Strict);
    private readonly ForexRateAlertService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexRateAlertServiceTests" /> class.
    /// </summary>
    public ForexRateAlertServiceTests()
    {
        _rateAlertRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(It.IsAny<int>()))
            .Returns(new List<RateAlert>());
        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(new List<RateAlert>());
        _rateAlertRepository
            .Setup(creates => creates.Create(It.IsAny<RateAlert>()))
            .Returns((RateAlert alert) => alert);
        _rateAlertRepository
            .Setup(deletes => deletes.Delete(It.IsAny<int>()))
            .Returns(Result.Success);
        _rateAlertRepository
            .Setup(markTriggered => markTriggered.MarkTriggered(It.IsAny<int>()))
            .Returns((int id) => new RateAlert { Id = id, IsTriggered = true });

        _exchangeService
            .Setup(getsPreview =>
                getsPreview.GetRatePreview(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
            .Returns(new ForexTransactionResponse { ExchangeRate = EurUsdRate });

        _service = new ForexRateAlertService(_rateAlertRepository.Object, _exchangeService.Object);
    }

    /// <summary>
    ///     Verifies the GetAlerts_WhenUserHasNoAlerts_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public void GetAlerts_WhenUserHasNoAlerts_ReturnsEmptyList()
    {
        // Arrange
        _rateAlertRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(new List<RateAlert>());

        // Act
        ErrorOr<List<ForexRateAlertDto>> result = _service.GetAlerts(ValidUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetAlerts_WhenRepositoryFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void GetAlerts_WhenRepositoryFails_ReturnsError()
    {
        // Arrange
        _rateAlertRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<List<ForexRateAlertDto>> result = _service.GetAlerts(ValidUserId);

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the GetAlerts_WhenAlertsExist_ReturnsMappedDtos scenario.
    /// </summary>
    [Fact]
    public void GetAlerts_WhenAlertsExist_ReturnsMappedDtos()
    {
        // Arrange
        var alerts = new List<RateAlert>
        {
            new()
            {
                Id = ValidAlertId,
                UserId = ValidUserId,
                BaseCurrency = EurCurrency,
                TargetCurrency = UsdCurrency,
                TargetRate = ValidTargetRate,
                IsBuyAlert = true,
                IsTriggered = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _rateAlertRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(alerts);

        // Act
        ErrorOr<List<ForexRateAlertDto>> result = _service.GetAlerts(ValidUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().BaseCurrency.Should().Be(EurCurrency);
        result.Value.First().TargetRate.Should().Be(ValidTargetRate);
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenBaseCurrencyIsEmpty_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenBaseCurrencyIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = string.Empty,
            TargetCurrency = UsdCurrency,
            TargetRate = ValidTargetRate
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenTargetCurrencyIsEmpty_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenTargetCurrencyIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = EurCurrency,
            TargetCurrency = string.Empty,
            TargetRate = ValidTargetRate
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenCurrenciesAreTheSame_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenCurrenciesAreTheSame_ReturnsValidationError()
    {
        // Arrange
        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = EurCurrency,
            TargetCurrency = EurCurrency,
            TargetRate = ValidTargetRate
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenTargetRateIsZero_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenTargetRateIsZero_ReturnsValidationError()
    {
        // Arrange
        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            TargetRate = 0m
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenValidDto_ReturnsCreatedAlert scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenValidDto_ReturnsCreatedAlert()
    {
        // Arrange
        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            TargetRate = ValidTargetRate,
            IsBuyAlert = true
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BaseCurrency.Should().Be(EurCurrency);
        result.Value.TargetCurrency.Should().Be(UsdCurrency);
        result.Value.TargetRate.Should().Be(ValidTargetRate);
        result.Value.IsTriggered.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the CreateAlert_WhenRepositoryFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void CreateAlert_WhenRepositoryFails_ReturnsError()
    {
        // Arrange
        _rateAlertRepository
            .Setup(creates => creates.Create(It.IsAny<RateAlert>()))
            .Returns(Error.Failure());

        var dto = new ForexRateAlertDto
        {
            UserId = ValidUserId,
            BaseCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            TargetRate = ValidTargetRate
        };

        // Act
        ErrorOr<ForexRateAlertDto> result = _service.CreateAlert(dto);

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the DeleteAlert_WhenAlertExists_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void DeleteAlert_WhenAlertExists_ReturnsSuccess()
    {
        // Arrange
        _rateAlertRepository
            .Setup(deletes => deletes.Delete(ValidAlertId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.DeleteAlert(ValidAlertId);

        // Assert
        result.IsError.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the DeleteAlert_WhenAlertDoesNotExist_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void DeleteAlert_WhenAlertDoesNotExist_ReturnsError()
    {
        // Arrange
        _rateAlertRepository
            .Setup(deletes => deletes.Delete(NonExistentAlertId))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = _service.DeleteAlert(NonExistentAlertId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the ProcessAlerts_WhenNoUntriggeredAlertsExist_ReturnsZero scenario.
    /// </summary>
    [Fact]
    public void ProcessAlerts_WhenNoUntriggeredAlertsExist_ReturnsZero()
    {
        // Arrange
        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(new List<RateAlert>());

        // Act
        ErrorOr<int> result = _service.ProcessAlerts();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
    }

    /// <summary>
    ///     Verifies the ProcessAlerts_WhenRepositoryFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ProcessAlerts_WhenRepositoryFails_ReturnsError()
    {
        // Arrange
        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(Error.Failure());

        // Act
        ErrorOr<int> result = _service.ProcessAlerts();

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the ProcessAlerts_WhenBuyAlertRateConditionMet_TriggersAlert scenario.
    /// </summary>
    [Fact]
    public void ProcessAlerts_WhenBuyAlertRateConditionMet_TriggersAlert()
    {
        // Arrange
        var alerts = new List<RateAlert>
        {
            new()
            {
                Id = ValidAlertId,
                UserId = ValidUserId,
                BaseCurrency = EurCurrency,
                TargetCurrency = UsdCurrency,
                TargetRate = BuyAlertTargetRateAboveCurrent,
                IsBuyAlert = true,
                IsTriggered = false
            }
        };

        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(alerts);

        _exchangeService
            .Setup(getsPreview => getsPreview.GetRatePreview(EurCurrency, UsdCurrency, 1m))
            .Returns(new ForexTransactionResponse { ExchangeRate = EurUsdRate });

        // Act
        ErrorOr<int> result = _service.ProcessAlerts();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        _rateAlertRepository.Verify(markTriggered => markTriggered.MarkTriggered(ValidAlertId), Times.Once);
    }

    /// <summary>
    ///     Verifies the ProcessAlerts_WhenSellAlertRateConditionMet_TriggersAlert scenario.
    /// </summary>
    [Fact]
    public void ProcessAlerts_WhenSellAlertRateConditionMet_TriggersAlert()
    {
        // Arrange
        var alerts = new List<RateAlert>
        {
            new()
            {
                Id = ValidAlertId,
                UserId = ValidUserId,
                BaseCurrency = EurCurrency,
                TargetCurrency = UsdCurrency,
                TargetRate = SellAlertTargetRateBelowCurrent,
                IsBuyAlert = false,
                IsTriggered = false
            }
        };

        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(alerts);

        _exchangeService
            .Setup(getsPreview => getsPreview.GetRatePreview(EurCurrency, UsdCurrency, 1m))
            .Returns(new ForexTransactionResponse { ExchangeRate = EurUsdRate });

        // Act
        ErrorOr<int> result = _service.ProcessAlerts();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        _rateAlertRepository.Verify(markTriggered => markTriggered.MarkTriggered(ValidAlertId), Times.Once);
    }

    /// <summary>
    ///     Verifies the ProcessAlerts_WhenConditionNotMet_DoesNotTriggerAlert scenario.
    /// </summary>
    [Fact]
    public void ProcessAlerts_WhenConditionNotMet_DoesNotTriggerAlert()
    {
        // Arrange
        var alerts = new List<RateAlert>
        {
            new()
            {
                Id = ValidAlertId,
                UserId = ValidUserId,
                BaseCurrency = EurCurrency,
                TargetCurrency = UsdCurrency,
                TargetRate = 1.50m,
                IsBuyAlert = false,
                IsTriggered = false
            }
        };

        _rateAlertRepository
            .Setup(getsUntriggered => getsUntriggered.GetUntriggeredAlerts())
            .Returns(alerts);

        _exchangeService
            .Setup(getsPreview => getsPreview.GetRatePreview(EurCurrency, UsdCurrency, 1m))
            .Returns(new ForexTransactionResponse { ExchangeRate = EurUsdRate });

        // Act
        ErrorOr<int> result = _service.ProcessAlerts();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
        _rateAlertRepository.Verify(markTriggered => markTriggered.MarkTriggered(It.IsAny<int>()), Times.Never);
    }
}
