namespace BankingApp.Application.Features.BillPayments.Commands;

using Common.Logging;
using Common.Security;
using Contracts.Features.BillPayments.Dtos;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.AccountAggregate.Entities;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.ReferenceData.Billers;
using Domain.Repositories;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;
using Money = NodaMoney.Money;

public sealed record ProcessBillPaymentCommand(
    int UserId,
    int SourceAccountId,
    int BillerId,
    string BillerReference,
    decimal Amount,
    string? TwoFaToken)
    : IRequest<ErrorOr<BillPayResponse>>;

public sealed class ProcessBillPaymentCommandHandler(
    IAccountRepository accountRepository,
    IBillPaymentRepository billPaymentRepository,
    IBillerRepository billerRepository,
    IOtpService otpService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<ProcessBillPaymentCommandHandler> logger)
    : IRequestHandler<ProcessBillPaymentCommand, ErrorOr<BillPayResponse>>
{
    public async Task<ErrorOr<BillPayResponse>> Handle(ProcessBillPaymentCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<Account> accountResult = await GetValidAccountAsync(command, cancellationToken);
        if (accountResult.IsError)
        {
            return accountResult.FirstError;
        }

        ErrorOr<Biller> billerResult = await GetBillerAsync(command.BillerId, cancellationToken);
        if (billerResult.IsError)
        {
            return billerResult.FirstError;
        }

        Account account = accountResult.Value;

        ErrorOr<BillPayment> paymentResult = BillPayment.Create(
            command.UserId,
            command.SourceAccountId,
            command.BillerId,
            command.BillerReference,
            CreateAmount(command.Amount, account.Currency),
            CreateFee(command.Amount, account.Currency),
            clock.UtcNow);

        if (paymentResult.IsError)
        {
            return paymentResult.FirstError;
        }

        BillPayment payment = paymentResult.Value;
        ErrorOr<Success> twoFactorResult = VerifyTwoFactorIfRequired(command, payment);
        if (twoFactorResult.IsError)
        {
            return twoFactorResult.FirstError;
        }

        DateTime now = clock.UtcNow;
        string receiptNumber = $"RCP-{now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        ErrorOr<Money> newBalanceResult = account.Debit(payment.TotalDebit, now);
        if (newBalanceResult.IsError)
        {
            return newBalanceResult.FirstError;
        }

        Transaction txn = account.RecordTransaction(
            receiptNumber,
            "BILL_PAYMENT",
            TransactionDirection.Out,
            payment.Amount,
            newBalanceResult.Value,
            TransactionStatus.Completed,
            now);

        payment.MarkProcessed(receiptNumber, txn.Id);

        await accountRepository.UpdateAsync(account, cancellationToken);
        await billPaymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BillPayResponse
        {
            Id = payment.Id,
            ReceiptNumber = payment.ReceiptNumber,
            Fee = payment.Fee.Amount,
            Amount = payment.Amount.Amount,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }

    private async Task<ErrorOr<Account>> GetValidAccountAsync(
        ProcessBillPaymentCommand command,
        CancellationToken cancellationToken)
    {
        Account? account = await accountRepository.GetByIdAsync(command.SourceAccountId, cancellationToken);
        if (account is null || account.UserId != command.UserId)
        {
            return AccountErrors.NotFound;
        }

        if (!account.IsActive())
        {
            return BillPaymentErrors.AccountNotActive;
        }

        return account;
    }

    private async Task<ErrorOr<Biller>> GetBillerAsync(int billerId, CancellationToken cancellationToken)
    {
        Biller? biller = await billerRepository.GetByIdAsync(billerId, cancellationToken);
        return biller is null ? BillPaymentErrors.NotFound : biller;
    }

    private static Money CreateAmount(decimal amount, NodaMoney.Currency currency) => new(amount, currency);

    private static Money CreateFee(decimal amount, NodaMoney.Currency currency) =>
        BillPaymentFeePolicy.Calculate(amount, currency);

    private ErrorOr<Success> VerifyTwoFactorIfRequired(ProcessBillPaymentCommand command, BillPayment payment)
    {
        if (!BillPayment.RequiresTwoFactorAuthentication(payment.Amount))
        {
            return Result.Success;
        }

        if (string.IsNullOrWhiteSpace(command.TwoFaToken))
        {
            return BillPaymentErrors.TwoFaRequired;
        }

        ErrorOr<bool> otpValid = otpService.VerifyTotp(command.UserId, command.TwoFaToken);
        if (otpValid.IsError || !otpValid.Value)
        {
            ApplicationLogMessages.BillPaymentTwoFactorInvalid(logger, command.UserId);
            return BillPaymentErrors.InvalidTwoFaToken;
        }

        return Result.Success;
    }
}

public sealed class ProcessBillPaymentCommandValidator : AbstractValidator<ProcessBillPaymentCommand>
{
    public ProcessBillPaymentCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.SourceAccountId).GreaterThan(0);
        RuleFor(command => command.BillerId).GreaterThan(0);
        RuleFor(command => command.BillerReference).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0m);
    }
}
