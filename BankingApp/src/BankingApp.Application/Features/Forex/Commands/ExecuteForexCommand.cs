namespace BankingApp.Application.Features.Forex.Commands;

using Common.Contracts;
using Common.Utilities;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.ForexAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using FluentValidation;
using MediatR;
using Currency = NodaMoney.Currency;
using Money = NodaMoney.Money;

public sealed record ExecuteForexCommand(
    int UserId,
    int SourceAccountId,
    int TargetAccountId,
    string SourceCurrency,
    string TargetCurrency,
    decimal SourceAmount)
    : IRequest<ErrorOr<ForexTransactionResponse>>;

public sealed class ExecuteForexCommandHandler(
    IAccountRepository accountRepository,
    IForexRepository forexRepository,
    ILockedRateCache lockedRateCache,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IRequestHandler<ExecuteForexCommand, ErrorOr<ForexTransactionResponse>>
{
    private const decimal CommissionRate = 0.005m;
    private static readonly TimeSpan _rateTtl = TimeSpan.FromSeconds(30);

    public async Task<ErrorOr<ForexTransactionResponse>> Handle(ExecuteForexCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<(Currency SourceCurrency, Currency TargetCurrency)> currencyPairResult = ParseCurrencyPair(command);
        if (currencyPairResult.IsError)
        {
            return currencyPairResult.FirstError;
        }

        Currency sourceCurrency = currencyPairResult.Value.SourceCurrency;
        Currency targetCurrency = currencyPairResult.Value.TargetCurrency;

        ErrorOr<LockedRate> lockedRateResult = GetValidLockedRate(command.UserId, sourceCurrency, targetCurrency);
        if (lockedRateResult.IsError)
        {
            return lockedRateResult.FirstError;
        }

        ErrorOr<(Account SourceAccount, Account TargetAccount)> accountsResult =
            await GetValidAccountsAsync(command, sourceCurrency, targetCurrency, cancellationToken);
        if (accountsResult.IsError)
        {
            return accountsResult.FirstError;
        }

        Account sourceAccount = accountsResult.Value.SourceAccount;
        Account targetAccount = accountsResult.Value.TargetAccount;
        decimal rate = lockedRateResult.Value.Rate;
        Money sourceAmount = CreateSourceAmount(command.SourceAmount, sourceCurrency);
        Money commission = CreateCommission(command.SourceAmount, sourceCurrency);
        Money targetAmount = CreateTargetAmount(command.SourceAmount, rate, targetCurrency);

        ErrorOr<ForexTransaction> forexResult = ForexTransaction.Create(
            command.UserId,
            command.SourceAccountId,
            command.TargetAccountId,
            sourceAmount,
            targetAmount,
            rate,
            commission,
            clock.UtcNow);

        if (forexResult.IsError)
        {
            return forexResult.FirstError;
        }

        DateTime now = clock.UtcNow;
        string forexRef = $"FX-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        ErrorOr<Money> sourceBalanceResult = sourceAccount.Debit(sourceAmount + commission, now);
        if (sourceBalanceResult.IsError)
        {
            return sourceBalanceResult.FirstError;
        }

        Transaction sourceTxn = sourceAccount.RecordTransaction(
            forexRef,
            "FOREX",
            TransactionDirection.Out,
            sourceAmount,
            sourceBalanceResult.Value,
            TransactionStatus.Completed,
            now);

        ErrorOr<Money> targetBalanceResult = targetAccount.Credit(targetAmount, now);
        if (targetBalanceResult.IsError)
        {
            return targetBalanceResult.FirstError;
        }

        Transaction targetTxn = targetAccount.RecordTransaction(
            forexRef,
            "FOREX",
            TransactionDirection.In,
            targetAmount,
            targetBalanceResult.Value,
            TransactionStatus.Completed,
            now);

        ForexTransaction forex = forexResult.Value;
        forex.MarkExecuted(sourceTxn.Id, targetTxn.Id);

        await accountRepository.UpdateAsync(sourceAccount, cancellationToken);
        await accountRepository.UpdateAsync(targetAccount, cancellationToken);
        await forexRepository.AddAsync(forex, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        lockedRateCache.Remove(command.UserId);

        return new ForexTransactionResponse
        {
            Id = forex.Id,
            SourceCurrency = sourceCurrency.Code,
            TargetCurrency = targetCurrency.Code,
            TargetAmount = targetAmount.Amount,
            ExchangeRate = rate,
            Commission = commission.Amount,
            Status = ExchangeTransactionStatus.Completed
        };
    }

    private static ErrorOr<(Currency SourceCurrency, Currency TargetCurrency)> ParseCurrencyPair(ExecuteForexCommand command)
    {
        try
        {
            var sourceCurrency = Currency.FromCode(command.SourceCurrency);
            var targetCurrency = Currency.FromCode(command.TargetCurrency);

            if (sourceCurrency == targetCurrency)
            {
                return ForexErrors.SameCurrency;
            }

            return (sourceCurrency, targetCurrency);
        }
        catch
        {
            return ForexErrors.InvalidCurrency;
        }
    }

    private ErrorOr<LockedRate> GetValidLockedRate(int userId, Currency sourceCurrency, Currency targetCurrency)
    {
        LockedRate? lockedRate = lockedRateCache.TryGet(userId);
        if (lockedRate is null || clock.UtcNow - lockedRate.LockedAt > _rateTtl)
        {
            return ForexErrors.RateExpired;
        }

        if (lockedRate.BaseCurrency != sourceCurrency || lockedRate.QuoteCurrency != targetCurrency)
        {
            return ForexErrors.LockedRateMismatch;
        }

        return lockedRate;
    }

    private async Task<ErrorOr<(Account SourceAccount, Account TargetAccount)>> GetValidAccountsAsync(
        ExecuteForexCommand command,
        Currency sourceCurrency,
        Currency targetCurrency,
        CancellationToken cancellationToken)
    {
        Account? sourceAccount = await accountRepository.GetByIdAsync(command.SourceAccountId, cancellationToken);
        if (sourceAccount is null || sourceAccount.UserId != command.UserId)
        {
            return AccountErrors.NotFound;
        }

        Account? targetAccount = await accountRepository.GetByIdAsync(command.TargetAccountId, cancellationToken);
        if (targetAccount is null || targetAccount.UserId != command.UserId)
        {
            return AccountErrors.NotFound;
        }

        if (!sourceAccount.IsActive() || !targetAccount.IsActive())
        {
            return AccountErrors.NotActive;
        }

        if (!sourceAccount.UsesCurrency(sourceCurrency) || !targetAccount.UsesCurrency(targetCurrency))
        {
            return ForexErrors.AccountCurrencyMismatch;
        }

        return (sourceAccount, targetAccount);
    }

    private static Money CreateSourceAmount(decimal amount, Currency currency) => new(amount, currency);

    private static Money CreateCommission(decimal amount, Currency currency) =>
        new(Math.Round(amount * CommissionRate, 2), currency);

    private static Money CreateTargetAmount(decimal amount, decimal rate, Currency currency) =>
        new(Math.Round(amount * rate, 2), currency);
}

public sealed class ExecuteForexCommandValidator : AbstractValidator<ExecuteForexCommand>
{
    public ExecuteForexCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.SourceAccountId).GreaterThan(0);
        RuleFor(command => command.TargetAccountId).GreaterThan(0);
        RuleFor(command => command.SourceAmount).GreaterThan(0m);
        RuleFor(command => command.SourceCurrency).Length(3);
        RuleFor(command => command.TargetCurrency).Length(3);
        RuleFor(command => command)
            .Must(command => !string.Equals(command.SourceCurrency, command.TargetCurrency, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ForexErrors.SameCurrency.Description);
    }
}
