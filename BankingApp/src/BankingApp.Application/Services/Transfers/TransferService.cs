// <copyright file="TransferService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferService class.
// </summary>

using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.Mapping;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Security;
using BankingApp.Application.Services.Transfers;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Transfers;

/// <summary>
///     Handles transfer creation and history retrieval.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="TransferService" /> class.
/// </remarks>
/// <param name="dashboardRepository">The dashboard repository.</param>
/// <param name="otpService">The OTP service for 2FA verification.</param>
/// <param name="logger">The logger.</param>
public class TransferService(
    IDashboardRepository dashboardRepository,
    IOtpService otpService,
    ILogger<TransferService> logger)
    : ITransferService
{
    private const decimal TwoFaAmountThreshold = 1000m;
    private const int ExpectedCurrencyCodeLength = 3;
    private const int IbanMinLength = 15;
    private const int IbanMaxLength = 34;
    private const int IbanCountryCodeLength = 2;
    private const string TransferRelatedEntityType = "Transfer";

    private readonly IDashboardRepository _dashboardRepository = dashboardRepository;
    private readonly IOtpService _otpService = otpService;
    private readonly ILogger<TransferService> _logger = logger;

    /// <inheritdoc />
    /// <param name="request">The request value.</param>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<TransferResponse> CreateTransfer(CreateTransferRequest request, int userId)
    {
        ErrorOr<Success> validationResult = ValidateRequest(request);
        if (validationResult.IsError)
        {
            return validationResult.FirstError;
        }

        ErrorOr<List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId);
        if (accountsResult.IsError)
        {
            _logger.LogWarning(
                "Transfer failed: could not retrieve accounts for user {UserId}.",
                userId);
            return TransferErrors.AccountNotFound;
        }

        Account? account = accountsResult.Value.FirstOrDefault(a => a.Id == request.SourceAccountId);
        if (account is null)
        {
            _logger.LogWarning(
                "Transfer failed: account {AccountId} not found for user {UserId}.",
                request.SourceAccountId,
                userId);
            return TransferErrors.AccountNotFound;
        }

        if (account.Status != AccountStatus.Active)
        {
            _logger.LogWarning(
                "Transfer failed: account {AccountId} is not active.",
                request.SourceAccountId);
            return TransferErrors.AccountNotActive;
        }

        if (account.Balance < request.Amount)
        {
            _logger.LogWarning(
                "Transfer failed: insufficient funds on account {AccountId}.",
                request.SourceAccountId);
            return TransferErrors.InsufficientFunds;
        }

        ErrorOr<Success> authResult = CheckTwoFa(request, userId);
        if (authResult.IsError)
        {
            return authResult.FirstError;
        }

        return DebitAndPersist(request, userId, account);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<TransferResponse>> GetHistory(int userId)
    {
        ErrorOr<List<Transfer>> result = _dashboardRepository.GetTransfersByUserId(userId);
        if (result.IsError)
        {
            _logger.LogError("Failed to retrieve transfer history for user {UserId}.", userId);
            return result.FirstError;
        }

        return result.Value
            .Select(MapToResponse)
            .ToList();
    }

    private static bool IsValidIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        if (iban.Length < IbanMinLength || iban.Length > IbanMaxLength)
        {
            return false;
        }

        if (!char.IsLetter(iban[0]) || !char.IsLetter(iban[1]))
        {
            return false;
        }

        if (!char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
        {
            return false;
        }

        return true;
    }

    private static string InferBankName(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban) || iban.Length < IbanCountryCodeLength)
        {
            return "Unknown Bank";
        }

        return iban[..IbanCountryCodeLength].ToUpperInvariant() switch
        {
            "RO" => "Romanian Bank",
            "DE" => "German Bank",
            "GB" => "UK Bank",
            "FR" => "French Bank",
            "US" => "US Bank",
            _ => "International Bank",
        };
    }

    private static string GenerateTransactionRef()
    {
        return $"TRF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    }

    private static TransferResponse MapToResponse(Transfer transfer)
    {
        return new TransferResponse
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccountId,
            TransactionId = transfer.TransactionId,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            RecipientBankName = transfer.RecipientBankName,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            Fee = transfer.Fee,
            Reference = transfer.Reference,
            Status = DomainEnumMapper.ToApplication(transfer.Status),
            CreatedAt = transfer.CreatedAt,
        };
    }

    private ErrorOr<Success> ValidateRequest(CreateTransferRequest request)
    {
        if (!IsValidIban(request.RecipientIban))
        {
            return TransferErrors.InvalidIban;
        }

        if (request.Amount <= 0)
        {
            return TransferErrors.InvalidAmount;
        }

        if (string.IsNullOrWhiteSpace(request.Currency)
            || request.Currency.Length != ExpectedCurrencyCodeLength)
        {
            return TransferErrors.InvalidCurrency;
        }

        return Result.Success;
    }

    private ErrorOr<Success> CheckTwoFa(CreateTransferRequest request, int userId)
    {
        if (request.Amount < TwoFaAmountThreshold)
        {
            return Result.Success;
        }

        if (string.IsNullOrWhiteSpace(request.TwoFaToken))
        {
            _logger.LogWarning(
                "Transfer rejected: 2FA token missing for amount {Amount}, user {UserId}.",
                request.Amount,
                userId);
            return TransferErrors.TwoFaRequired;
        }

        ErrorOr<bool> verifyResult = _otpService.VerifyTotp(userId, request.TwoFaToken);
        if (verifyResult.IsError || !verifyResult.Value)
        {
            _logger.LogWarning(
                "Transfer rejected: invalid 2FA token for user {UserId}.",
                userId);
            return TransferErrors.InvalidTwoFaToken;
        }

        return Result.Success;
    }

    private ErrorOr<TransferResponse> DebitAndPersist(
        CreateTransferRequest request,
        int userId,
        Account account)
    {
        ErrorOr<Success> debitResult = _dashboardRepository.DebitAccount(account.Id, request.Amount);
        if (debitResult.IsError)
        {
            _logger.LogError(
                "Transfer failed: could not debit account {AccountId}.",
                account.Id);
            return TransferErrors.DebitFailed;
        }

        decimal balanceAfter = account.Balance - request.Amount;

        var transaction = new Transaction
        {
            AccountId = account.Id,
            TransactionRef = GenerateTransactionRef(),
            Direction = TransactionDirection.Out,
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceAfter = balanceAfter,
            CounterpartyName = request.RecipientName,
            CounterpartyIban = request.RecipientIban,
            Fee = 0m,
            Status = TransactionStatus.Completed,
            RelatedEntityType = TransferRelatedEntityType,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<Transaction> logResult = _dashboardRepository.AddTransaction(transaction);
        if (logResult.IsError)
        {
            _logger.LogError(
                "Transfer failed: could not log transaction for account {AccountId}.",
                account.Id);
            return TransferErrors.TransactionLogFailed;
        }

        var transfer = new Transfer
        {
            UserId = userId,
            SourceAccountId = account.Id,
            TransactionId = logResult.Value.Id,
            RecipientName = request.RecipientName,
            RecipientIban = request.RecipientIban,
            RecipientBankName = InferBankName(request.RecipientIban),
            Amount = request.Amount,
            Currency = request.Currency,
            Fee = 0m,
            Reference = request.Reference,
            Status = TransferStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<Transfer> persistResult = _dashboardRepository.AddTransfer(transfer);
        if (persistResult.IsError)
        {
            _logger.LogError(
                "Transfer failed: could not persist transfer record for user {UserId}.",
                userId);
            return TransferErrors.PersistenceFailed;
        }

        return MapToResponse(persistResult.Value);
    }
}