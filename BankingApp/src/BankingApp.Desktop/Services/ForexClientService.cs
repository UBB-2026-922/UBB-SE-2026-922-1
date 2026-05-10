namespace BankingApp.Desktop.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Exchange;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Utilities;

/// <summary>
///     Implements <see cref="IForexClientService" /> with desktop-side business logic and proxy repositories.
/// </summary>
internal sealed class ForexClientService(
    ICurrentSession currentSession,
    IExchangeRepository exchangeRepository,
    IBillPaymentRepository billPaymentRepository)
    : IForexClientService
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

    private static readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(CacheDurationSeconds);

    private readonly ICurrentSession _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));
    private readonly IBillPaymentRepository _billPaymentRepository = billPaymentRepository ?? throw new ArgumentNullException(nameof(billPaymentRepository));
    private readonly IExchangeRepository _exchangeRepository = exchangeRepository ?? throw new ArgumentNullException(nameof(exchangeRepository));
    private readonly Dictionary<int, LockedRate> _lockedRates = [];
    private Dictionary<string, decimal>? _cachedRates;
    private DateTime _ratesLastFetched;

    public int? CurrentUserId => _currentSession.CurrentUserId;

    public Task<ErrorOr<ExchangeTransactionResponse>> GetPreviewAsync(
        string sourceCurrency,
        string targetCurrency,
        decimal amount)
    {
        if (string.IsNullOrWhiteSpace(sourceCurrency))
        {
            return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(Error.Validation(description: "Source currency cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(Error.Validation(description: "Target currency cannot be empty."));
        }

        if (string.Equals(sourceCurrency, targetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(Error.Validation(description: "Source and target currencies must differ."));
        }

        if (amount <= 0)
        {
            return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(Error.Validation(description: "Amount must be greater than zero."));
        }

        int? userId = _currentSession.GetCurrentUserId();
        if (userId is not null)
        {
            ErrorOr<decimal> lockResult = GetRate(sourceCurrency, targetCurrency);
            if (!lockResult.IsError)
            {
                _lockedRates[userId.Value] = new LockedRate
                {
                    User = new User { Id = userId.Value },
                    CurrencyPair = $"{sourceCurrency}/{targetCurrency}",
                    Rate = lockResult.Value,
                    LockedAt = DateTime.UtcNow,
                };
            }
        }

        ErrorOr<decimal> rateResult = GetRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(rateResult.FirstError);
        }

        decimal commission = CalculateCommission(amount);
        return Task.FromResult<ErrorOr<ExchangeTransactionResponse>>(new ExchangeTransactionResponse
        {
            SourceCurrency = sourceCurrency,
            TargetCurrency = targetCurrency,
            TargetAmount = amount * rateResult.Value - commission,
            ExchangeRate = rateResult.Value,
            Commission = commission,
            Status = ExchangeTransactionStatus.Pending,
        });
    }

    public async Task<ErrorOr<ExchangeTransactionResponse>> ExecuteExchangeAsync(ExchangeTransactionRequest request)
    {
        if (request.UserId <= 0 || !_lockedRates.TryGetValue(request.UserId, out LockedRate? lockedRate) || lockedRate.IsExpired())
        {
            return Error.Validation(description: "No valid rate lock found or the lock window has expired.");
        }

        if (request.SourceAccountId <= 0 || request.TargetAccountId <= 0)
        {
            var accounts = (await _billPaymentRepository.GetAccountsByUserIdAsync(request.UserId).ConfigureAwait(false)).ToList();
            request.SourceAccountId = request.SourceAccountId > 0
                ? request.SourceAccountId
                : accounts.FirstOrDefault(account =>
                    string.Equals(account.Currency, request.SourceCurrency, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            request.TargetAccountId = request.TargetAccountId > 0
                ? request.TargetAccountId
                : accounts.FirstOrDefault(account =>
                    string.Equals(account.Currency, request.TargetCurrency, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }

        if (request.SourceAccountId <= 0 || request.TargetAccountId <= 0)
        {
            return Error.NotFound(description: "Matching source and target accounts were not found for the requested currencies.");
        }

        decimal commission = CalculateCommission(request.SourceAmount);
        decimal targetAmount = request.SourceAmount * lockedRate.Rate - commission;

        var exchange = new ExchangeTransaction
        {
            User = new User { Id = request.UserId },
            SourceAccount = new Account { Id = request.SourceAccountId },
            TargetAccount = new Account { Id = request.TargetAccountId },
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
            return createResult.FirstError;
        }

        _lockedRates.Remove(request.UserId);

        return new ExchangeTransactionResponse
        {
            Id = createResult.Value.Id,
            SourceCurrency = createResult.Value.SourceCurrency,
            TargetCurrency = createResult.Value.TargetCurrency,
            TargetAmount = createResult.Value.TargetAmount,
            ExchangeRate = createResult.Value.ExchangeRate,
            Commission = createResult.Value.Commission,
            Status = createResult.Value.Status,
        };
    }

    private static decimal CalculateCommission(decimal amount)
        => Math.Max(MinimumCommission, amount * CommissionRate);

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
        if (_cachedRates != null && DateTime.UtcNow - _ratesLastFetched < _cacheDuration)
        {
            return _cachedRates;
        }

        Dictionary<string, decimal> rates = new()
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
