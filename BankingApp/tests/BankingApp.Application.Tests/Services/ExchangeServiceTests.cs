// <copyright file="ExchangeServiceTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.TeamB;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="ExchangeService" />.
/// </summary>
public class ExchangeServiceTests
{
    private const int ValidUserId = 1;
    private const int ValidSourceAccountId = 10;
    private const int ValidTargetAccountId = 20;
    private const string EurCurrency = "EUR";
    private const string UsdCurrency = "USD";
    private const string RonCurrency = "RON";
    private const decimal ValidAmount = 100m;
    private const decimal MinimumCommission = 0.50m;
    private const decimal CommissionRate = 0.005m;
    private const decimal SmallAmount = 10m;
    private const decimal LargeAmount = 1000m;
    private const decimal EurUsdRate = 1.15m;
    private const int NonExistentUserId = 99;

    private readonly Mock<IExchangeRepository> _exchangeRepository = new(MockBehavior.Strict);
    private readonly ExchangeService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeServiceTests" /> class.
    /// </summary>
    public ExchangeServiceTests()
    {
        _exchangeRepository
            .Setup(createsExchange => createsExchange.Create(It.IsAny<ExchangeTransaction>()))
            .Returns((ExchangeTransaction exchange) => exchange);
        _exchangeRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(It.IsAny<int>()))
            .Returns(new List<ExchangeTransaction>());

        _service = new ExchangeService(_exchangeRepository.Object);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenSourceCurrencyIsEmpty_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenSourceCurrencyIsEmpty_ReturnsValidationError()
    {
        // Arrange
        string sourceCurrency = string.Empty;

        // Act
        ErrorOr<ExchangeTransactionResponseDto> result =
            _service.GetRatePreview(sourceCurrency, UsdCurrency, ValidAmount);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenTargetCurrencyIsEmpty_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenTargetCurrencyIsEmpty_ReturnsValidationError()
    {
        // Arrange
        string targetCurrency = string.Empty;

        // Act
        ErrorOr<ExchangeTransactionResponseDto> result =
            _service.GetRatePreview(EurCurrency, targetCurrency, ValidAmount);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenCurrenciesAreTheSame_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenCurrenciesAreTheSame_ReturnsValidationError()
    {
        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.GetRatePreview(EurCurrency, EurCurrency, ValidAmount);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenAmountIsZero_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenAmountIsZero_ReturnsValidationError()
    {
        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.GetRatePreview(EurCurrency, UsdCurrency, 0m);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenCurrencyPairIsUnsupported_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenCurrencyPairIsUnsupported_ReturnsNotFoundError()
    {
        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.GetRatePreview("JPY", "CHF", ValidAmount);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenValidEurToUsdRequest_ReturnsPreviewWithCorrectRate scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenValidEurToUsdRequest_ReturnsPreviewWithCorrectRate()
    {
        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.GetRatePreview(EurCurrency, UsdCurrency, ValidAmount);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ExchangeRate.Should().Be(EurUsdRate);
        result.Value.SourceCurrency.Should().Be(EurCurrency);
        result.Value.TargetCurrency.Should().Be(UsdCurrency);
    }

    /// <summary>
    ///     Verifies the GetRatePreview_WhenValidRequest_ReturnsPreviewWithCommissionDeducted scenario.
    /// </summary>
    [Fact]
    public void GetRatePreview_WhenValidRequest_ReturnsPreviewWithCommissionDeducted()
    {
        // Arrange
        decimal expectedCommission = Math.Max(MinimumCommission, ValidAmount * CommissionRate);
        decimal expectedTarget = ValidAmount * EurUsdRate - expectedCommission;

        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.GetRatePreview(EurCurrency, UsdCurrency, ValidAmount);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Commission.Should().Be(expectedCommission);
        result.Value.TargetAmount.Should().Be(expectedTarget);
    }

    /// <summary>
    ///     Verifies the CalculateCommission_WhenAmountProducesFeeBelowMinimum_ReturnsMinimumCommission scenario.
    /// </summary>
    [Fact]
    public void CalculateCommission_WhenAmountProducesFeeBelowMinimum_ReturnsMinimumCommission()
    {
        // Act
        decimal commission = ExchangeService.CalculateCommission(SmallAmount);

        // Assert
        commission.Should().Be(MinimumCommission);
    }

    /// <summary>
    ///     Verifies the CalculateCommission_WhenAmountProducesFeeAboveMinimum_ReturnsPercentageCommission scenario.
    /// </summary>
    [Fact]
    public void CalculateCommission_WhenAmountProducesFeeAboveMinimum_ReturnsPercentageCommission()
    {
        // Arrange
        decimal expectedCommission = LargeAmount * CommissionRate;

        // Act
        decimal commission = ExchangeService.CalculateCommission(LargeAmount);

        // Assert
        commission.Should().Be(expectedCommission);
    }

    /// <summary>
    ///     Verifies the IsRateLockValid_WhenNoLockExists_ReturnsFalse scenario.
    /// </summary>
    [Fact]
    public void IsRateLockValid_WhenNoLockExists_ReturnsFalse()
    {
        // Act
        bool result = _service.IsRateLockValid(NonExistentUserId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the LockRate_WhenValidCurrencyPair_ReturnsLockedRateWithCorrectValues scenario.
    /// </summary>
    [Fact]
    public void LockRate_WhenValidCurrencyPair_ReturnsLockedRateWithCorrectValues()
    {
        // Act
        ErrorOr<LockedRate> result = _service.LockRate(ValidUserId, EurCurrency, UsdCurrency);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UserId.Should().Be(ValidUserId);
        result.Value.Rate.Should().Be(EurUsdRate);
        result.Value.CurrencyPair.Should().Be($"{EurCurrency}/{UsdCurrency}");
    }

    /// <summary>
    ///     Verifies the IsRateLockValid_WhenLockExists_ReturnsTrue scenario.
    /// </summary>
    [Fact]
    public void IsRateLockValid_WhenLockExists_ReturnsTrue()
    {
        // Arrange
        _service.LockRate(ValidUserId, EurCurrency, UsdCurrency);

        // Act
        bool result = _service.IsRateLockValid(ValidUserId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the LockRate_WhenUnsupportedCurrencyPair_ReturnsNotFoundError scenario.
    /// </summary>
    [Fact]
    public void LockRate_WhenUnsupportedCurrencyPair_ReturnsNotFoundError()
    {
        // Act
        ErrorOr<LockedRate> result = _service.LockRate(ValidUserId, "JPY", "CHF");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    /// <summary>
    ///     Verifies the ExecuteExchange_WhenNoRateLockExists_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void ExecuteExchange_WhenNoRateLockExists_ReturnsValidationError()
    {
        // Arrange
        var request = new ExchangeTransactionRequestDto
        {
            UserId = NonExistentUserId,
            SourceAccountId = ValidSourceAccountId,
            TargetAccountId = ValidTargetAccountId,
            SourceCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            SourceAmount = ValidAmount
        };

        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.ExecuteExchange(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    /// <summary>
    ///     Verifies the ExecuteExchange_WhenValidLockExists_ReturnsCompletedExchange scenario.
    /// </summary>
    [Fact]
    public void ExecuteExchange_WhenValidLockExists_ReturnsCompletedExchange()
    {
        // Arrange
        _service.LockRate(ValidUserId, EurCurrency, UsdCurrency);

        var request = new ExchangeTransactionRequestDto
        {
            UserId = ValidUserId,
            SourceAccountId = ValidSourceAccountId,
            TargetAccountId = ValidTargetAccountId,
            SourceCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            SourceAmount = ValidAmount
        };

        // Act
        ErrorOr<ExchangeTransactionResponseDto> result = _service.ExecuteExchange(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(ExchangeTransactionStatus.Completed);
        result.Value.SourceCurrency.Should().Be(EurCurrency);
        result.Value.TargetCurrency.Should().Be(UsdCurrency);
    }

    /// <summary>
    ///     Verifies the ExecuteExchange_WhenValidLockExists_RemovesLockAfterExecution scenario.
    /// </summary>
    [Fact]
    public void ExecuteExchange_WhenValidLockExists_RemovesLockAfterExecution()
    {
        // Arrange
        _service.LockRate(ValidUserId, EurCurrency, UsdCurrency);

        var request = new ExchangeTransactionRequestDto
        {
            UserId = ValidUserId,
            SourceAccountId = ValidSourceAccountId,
            TargetAccountId = ValidTargetAccountId,
            SourceCurrency = EurCurrency,
            TargetCurrency = UsdCurrency,
            SourceAmount = ValidAmount
        };

        // Act
        _service.ExecuteExchange(request);
        bool lockStillValid = _service.IsRateLockValid(ValidUserId);

        // Assert
        lockStillValid.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the GetExchangeHistory_WhenUserHasNoTransactions_ReturnsEmptyList scenario.
    /// </summary>
    [Fact]
    public void GetExchangeHistory_WhenUserHasNoTransactions_ReturnsEmptyList()
    {
        // Arrange
        _exchangeRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(new List<ExchangeTransaction>());

        // Act
        ErrorOr<List<ExchangeTransactionResponseDto>> result = _service.GetExchangeHistory(ValidUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies the GetExchangeHistory_WhenRepositoryFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void GetExchangeHistory_WhenRepositoryFails_ReturnsError()
    {
        // Arrange
        _exchangeRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(Error.Failure());

        // Act
        ErrorOr<List<ExchangeTransactionResponseDto>> result = _service.GetExchangeHistory(ValidUserId);

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the GetExchangeHistory_WhenTransactionsExist_ReturnsMappedDtos scenario.
    /// </summary>
    [Fact]
    public void GetExchangeHistory_WhenTransactionsExist_ReturnsMappedDtos()
    {
        // Arrange
        var exchanges = new List<ExchangeTransaction>
        {
            new()
            {
                Id = 1,
                UserId = ValidUserId,
                SourceCurrency = EurCurrency,
                TargetCurrency = UsdCurrency,
                SourceAmount = ValidAmount,
                TargetAmount = 114.50m,
                ExchangeRate = EurUsdRate,
                Commission = MinimumCommission,
                Status = ExchangeTransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow
            }
        };

        _exchangeRepository
            .Setup(getsByUserId => getsByUserId.GetByUserId(ValidUserId))
            .Returns(exchanges);

        // Act
        ErrorOr<List<ExchangeTransactionResponseDto>> result = _service.GetExchangeHistory(ValidUserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value.First().SourceCurrency.Should().Be(EurCurrency);
        result.Value.First().ExchangeRate.Should().Be(EurUsdRate);
    }
}