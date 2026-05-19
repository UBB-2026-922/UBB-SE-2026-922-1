namespace BankingApp.Application.Features.Transfers.Commands;

using Common.Security;
using Contracts.Features.Transfers.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Aggregates.TransferAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;
using Money = NodaMoney.Money;
using Currency = NodaMoney.Currency;

public sealed record ExecuteTransferCommand(
    int UserId,
    int SourceAccountId,
    string RecipientName,
    string RecipientIban,
    decimal Amount,
    string Currency,
    string? Reference,
    string? TwoFaToken)
    : IRequest<ErrorOr<TransferResponse>>;

public sealed class ExecuteTransferCommandHandler(
    IAccountRepository accountRepository,
    ITransferRepository transferRepository,
    IBeneficiaryRepository beneficiaryRepository,
    IOtpService otpService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<ExecuteTransferCommandHandler> logger)
    : IRequestHandler<ExecuteTransferCommand, ErrorOr<TransferResponse>>
{
    private const decimal TransferFee = 1.00m;
    private const string TransferType = "TRANSFER";
    private const string TransferRef = "TRF";

    public async Task<ErrorOr<TransferResponse>> Handle(
        ExecuteTransferCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<Iban> ibanResult = Iban.Create(command.RecipientIban);
        if (ibanResult.IsError)
        {
            return ibanResult.FirstError;
        }

        ErrorOr<Currency> currencyResult = ParseCurrency(command.Currency);
        if (currencyResult.IsError)
        {
            return currencyResult.FirstError;
        }

        ErrorOr<Account> accountResult = await GetValidSourceAccountAsync(command, cancellationToken);
        if (accountResult.IsError)
        {
            return accountResult.FirstError;
        }

        Account account = accountResult.Value;
        if (!account.UsesCurrency(currencyResult.Value))
        {
            return TransferErrors.CurrencyMismatch;
        }

        DateTime now = clock.UtcNow;
        ErrorOr<Transfer> transferResult = CreateTransfer(command, ibanResult.Value, account.Currency, now);
        if (transferResult.IsError)
        {
            return transferResult.FirstError;
        }

        Transfer transfer = transferResult.Value;
        ErrorOr<Success> twoFactorResult = VerifyTwoFactorIfRequired(command, transfer);
        if (twoFactorResult.IsError)
        {
            return twoFactorResult.FirstError;
        }

        string transactionRef = CreateTransactionReference(now);
        ErrorOr<Money> newBalanceResult = account.Debit(transfer.TotalDebit, now);
        if (newBalanceResult.IsError)
        {
            ApplicationLogMessages.TransferInsufficientFunds(logger, command.SourceAccountId);
            return newBalanceResult.FirstError;
        }

        Transaction transaction = account.RecordTransaction(
            transactionRef,
            TransferType,
            TransactionDirection.Out,
            transfer.Amount,
            newBalanceResult.Value,
            TransactionStatus.Completed,
            now);

        transfer.MarkExecuted(transaction.Id, now.AddDays(1));

        await UpdateBeneficiaryStatsAsync(command.UserId, transfer.RecipientIban, transfer.Amount.Amount, now,
            cancellationToken);
        await accountRepository.UpdateAsync(account, cancellationToken);
        await transferRepository.AddAsync(transfer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(transfer, transactionRef);
    }

    private static ErrorOr<Currency> ParseCurrency(string currencyCode)
    {
        try
        {
            return Currency.FromCode(currencyCode);
        }
        catch
        {
            return TransferErrors.InvalidCurrency;
        }
    }

    private async Task<ErrorOr<Account>> GetValidSourceAccountAsync(
        ExecuteTransferCommand command,
        CancellationToken ct)
    {
        Account? account = await accountRepository.GetByIdAsync(command.SourceAccountId, ct);

        if (account is null || account.UserId != command.UserId)
        {
            ApplicationLogMessages.TransferAccountNotFound(logger, command.SourceAccountId, command.UserId);
            return TransferErrors.AccountNotFound;
        }

        if (!account.IsActive())
        {
            ApplicationLogMessages.TransferAccountNotActive(logger, command.SourceAccountId);
            return TransferErrors.AccountNotActive;
        }

        return account;
    }

    private static ErrorOr<Transfer> CreateTransfer(
        ExecuteTransferCommand command,
        Iban iban,
        Currency currency,
        DateTime createdAt)
    {
        Money amount = new(command.Amount, currency);
        Money fee = new(TransferFee, currency);

        return Transfer.Create(
            command.UserId,
            command.SourceAccountId,
            command.RecipientName,
            iban,
            amount,
            fee,
            command.Reference,
            createdAt);
    }

    private ErrorOr<Success> VerifyTwoFactorIfRequired(ExecuteTransferCommand command, Transfer transfer)
    {
        if (!Transfer.RequiresTwoFactorAuthentication(transfer.Amount))
        {
            return Result.Success;
        }

        if (string.IsNullOrWhiteSpace(command.TwoFaToken))
        {
            ApplicationLogMessages.TransferTwoFactorMissing(logger, command.Amount, command.UserId);
            return TransferErrors.TwoFaRequired;
        }

        ErrorOr<bool> otpValid = otpService.VerifyTotp(command.UserId, command.TwoFaToken);
        if (otpValid.IsError || !otpValid.Value)
        {
            ApplicationLogMessages.TransferTwoFactorInvalid(logger, command.UserId);
            return TransferErrors.InvalidTwoFaToken;
        }

        return Result.Success;
    }

    private static string CreateTransactionReference(DateTime now) =>
        $"{TransferRef}-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

    private static TransferResponse ToResponse(Transfer transfer, string transactionRef) =>
        new()
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccountId,
            TransactionId = transfer.LedgerTransactionId,
            TransactionRef = transactionRef,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban.Value,
            Amount = transfer.Amount.Amount,
            Currency = transfer.Amount.Currency.Code,
            Fee = transfer.Fee.Amount,
            Reference = transfer.Reference,
            Status = transfer.Status,
            CreatedAt = transfer.CreatedAt
        };

    private async Task UpdateBeneficiaryStatsAsync(
        int userId,
        Iban iban,
        decimal amount,
        DateTime now,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Beneficiary> beneficiaries =
            await beneficiaryRepository.ListByUserIdAsync(userId, cancellationToken);
        Beneficiary? beneficiary = beneficiaries.FirstOrDefault(b => b.Iban.Value == iban.Value);
        if (beneficiary is not null)
        {
            beneficiary.RegisterTransfer(amount, now);
            await beneficiaryRepository.UpdateAsync(beneficiary, cancellationToken);
        }
    }
}

public sealed class ExecuteTransferCommandValidator : AbstractValidator<ExecuteTransferCommand>
{
    public ExecuteTransferCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.SourceAccountId).GreaterThan(0);
        RuleFor(command => command.RecipientName).NotEmpty();
        RuleFor(command => command.RecipientIban).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0m);
        RuleFor(command => command.Currency).Length(3);
    }
}