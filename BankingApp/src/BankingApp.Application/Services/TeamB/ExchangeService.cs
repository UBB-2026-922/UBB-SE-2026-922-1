// <copyright file="ExchangeService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ExchangeService class.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Implements application-level operations for the FX currency exchange feature.
/// </summary>
public class ExchangeService : IExchangeService
{
    private const decimal CommissionRate = 0.005m;
    private const decimal MinimumCommission = 0.50m;
    private const int RatePrecisionDecimals = 2;
    private const int CacheDurationSeconds = 30;
    private const int BaseCurrencyIndex = 0;
    private const int TargetCurrencyIndex = 1;

    private const string EurUsdPair = "EUR/USD";
    private const string EurGbpPair = "EUR/GBP";
    private const string EurRonPair = "EUR/RON";
    private const string UsdRonPair = "USD/RON";
    private const string GbpRonPair = "GBP/RON";

    private const decimal EurUsdRate = 1.15m;
    private const decimal EurGbpRate = 0.86m;
    private const decimal EurRonRate = 5.09m;
    private const decimal UsdRonRate = 4.41m;
    private const decimal GbpRonRate = 5.90m;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(CacheDurationSeconds);

    private readonly IExchangeRepository _exchangeRepository;
    private readonly Dictionary<int, LockedRate> _lockedRates = new();

    private Dictionary<string, decimal>? _cachedRates;
    private DateTime _ratesLastFetched;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExchangeService" /> class.
    /// </summary>
    /// <param name="exchangeRepository">The exchange repository.</param>
    public ExchangeService(IExchangeRepository exchangeRepository)
    {
        _exchangeRepository = exchangeRepository;
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransactionResponseDto> GetRatePreview(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        if (string.IsNullOrWhiteSpace(sourceCurrency))
        {
            return Error.Validation(description: "Source currency cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            return Error.Validation(description: "Target currency cannot be empty.");
        }

        if (sourceCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(description: "Source and target currencies must differ.");
        }

        if (amount <= 0)
        {
            return Error.Validation(description: "Amount must be greater than zero.");
        }

        ErrorOr<decimal> rateResult = GetRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return rateResult.Errors;
        }

        decimal rate = rateResult.Value;
        decimal commission = CalculateCommission(amount);
        decimal targetAmount = (amount * rate) - commission;

        return new ExchangeTransactionResponseDto
        {
            SourceCurrency = sourceCurrency,
            TargetCurrency = targetCurrency,
            SourceAmount = amount,
            TargetAmount = targetAmount,
            ExchangeRate = rate,
            Commission = commission,
            Status = ExchangeTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransactionResponseDto> ExecuteExchange(ExchangeTransactionRequestDto request)
    {
        if (!IsRateLockValid(request.UserId))
        {
            return Error.Validation(description: "No valid rate lock found or the lock window has expired.");
        }

        LockedRate lockedRate = _lockedRates[request.UserId];
        decimal commission = CalculateCommission(request.SourceAmount);
        decimal targetAmount = (request.SourceAmount * lockedRate.Rate) - commission;

        ExchangeTransaction exchange = new ExchangeTransaction
        {
            UserId = request.UserId,
            SourceAccountId = request.SourceAccountId,
            TargetAccountId = request.TargetAccountId,
            SourceCurrency = request.SourceCurrency,
            TargetCurrency = request.TargetCurrency,
            SourceAmount = request.SourceAmount,
            TargetAmount = targetAmount,
            ExchangeRate = lockedRate.Rate,
            Commission = commission,
            RateLockedAt = lockedRate.LockedAt,
            Status = ExchangeTransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<ExchangeTransaction> createResult = _exchangeRepository.Create(exchange);
        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        _lockedRates.Remove(request.UserId);

        return MapToResponseDto(createResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<List<ExchangeTransactionResponseDto>> GetExchangeHistory(int userId)
    {
        ErrorOr<List<ExchangeTransaction>> result = _exchangeRepository.GetByUserId(userId);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Select(MapToResponseDto).ToList();
    }

    /// <summary>Locks in the current rate for a user for 30 seconds.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="sourceCurrency">The source currency code.</param>
    /// <param name="targetCurrency">The target currency code.</param>
    /// <returns>The locked rate, or an error.</returns>
    public ErrorOr<LockedRate> LockRate(int userId, string sourceCurrency, string targetCurrency)
    {
        ErrorOr<decimal> rateResult = GetRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return rateResult.Errors;
        }

        LockedRate lockedRate = new LockedRate
        {
            UserId = userId,
            CurrencyPair = $"{sourceCurrency}/{targetCurrency}",
            Rate = rateResult.Value,
            LockedAt = DateTime.UtcNow,
        };

        _lockedRates[userId] = lockedRate;
        return lockedRate;
    }

    /// <summary>Returns whether a user has a valid unexpired rate lock.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>True if lock exists and is not expired.</returns>
    public bool IsRateLockValid(int userId)
    {
        if (!_lockedRates.TryGetValue(userId, out LockedRate? lockedRate))
        {
            return false;
        }

        return !lockedRate.IsExpired();
    }

    /// <summary>Calculates the commission — 0.5% of amount, minimum 0.50.</summary>
    /// <param name="amount">The source amount.</param>
    /// <returns>The commission value.</returns>
    public decimal CalculateCommission(decimal amount)
    {
        return Math.Max(MinimumCommission, amount * CommissionRate);
    }

    private static ExchangeTransactionResponseDto MapToResponseDto(ExchangeTransaction exchange)
    {
        return new ExchangeTransactionResponseDto
        {
            Id = exchange.Id,
            SourceCurrency = exchange.SourceCurrency,
            TargetCurrency = exchange.TargetCurrency,
            SourceAmount = exchange.SourceAmount,
            TargetAmount = exchange.TargetAmount,
            ExchangeRate = exchange.ExchangeRate,
            Commission = exchange.Commission,
            Status = exchange.Status,
            CreatedAt = exchange.CreatedAt,
        };
    }

    private ErrorOr<decimal> GetRate(string sourceCurrency, string targetCurrency)
    {
        Dictionary<string, decimal> rates = GetLiveRates();
        string key = $"{sourceCurrency}/{targetCurrency}";

        if (rates.TryGetValue(key, out decimal rate))
        {
            return rate;
        }

        string inverseKey = $"{targetCurrency}/{sourceCurrency}";
        if (rates.TryGetValue(inverseKey, out decimal inverseRate))
        {
            return Math.Round(1 / inverseRate, RatePrecisionDecimals);
        }

        return Error.NotFound(description: $"Rate not found for pair {sourceCurrency}/{targetCurrency}.");
    }

    private Dictionary<string, decimal> GetLiveRates()
    {
        if (_cachedRates != null && DateTime.UtcNow - _ratesLastFetched < CacheDuration)
        {
            return _cachedRates;
        }

        Dictionary<string, decimal> rates = new Dictionary<string, decimal>
        {
            { EurUsdPair, EurUsdRate },
            { EurGbpPair, EurGbpRate },
            { EurRonPair, EurRonRate },
            { UsdRonPair, UsdRonRate },
            { GbpRonPair, GbpRonRate },
        };

        foreach (string pair in rates.Keys.ToList())
        {
            string[] parts = pair.Split('/');
            string inverseKey = $"{parts[TargetCurrencyIndex]}/{parts[BaseCurrencyIndex]}";
            rates[inverseKey] = Math.Round(1 / rates[pair], RatePrecisionDecimals);
        }

        _cachedRates = rates;
        _ratesLastFetched = DateTime.UtcNow;
        return _cachedRates;
    }
}