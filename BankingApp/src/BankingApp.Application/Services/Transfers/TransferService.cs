namespace BankingApp.Application.Services.Transfers;

using DTOs.Transfer;
using Logging;
using Repositories.Interfaces;
using Security;
using Domain.Entities;
using Domain.Enums;
using Domain.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;

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
    private const int ExchangeRatePrecision = 4;
    private const string TransferRelatedEntityType = "Transfer";
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

    private readonly IDashboardRepository _dashboardRepository = dashboardRepository;
    private readonly IOtpService _otpService = otpService;
    private readonly ILogger<TransferService> _logger = logger;

    /// <inheritdoc />
    public ErrorOr<List<TransferAccountSelectionResponse>> GetAvailableAccounts(int userId)
    {
        ErrorOr<List<Account>> accountsResult = _dashboardRepository.GetAccountsByUser(userId);
        if (accountsResult.IsError)
        {
            return accountsResult.FirstError;
        }

        return accountsResult.Value
            .Where(account => account.Status == AccountStatus.Active)
            .OrderBy(account => account.AccountName)
            .Select(account => new TransferAccountSelectionResponse
            {
                Id = account.Id,
                Iban = account.Iban,
                Currency = account.Currency,
                Balance = account.Balance,
                AccountName = account.AccountName ?? string.Empty
            })
            .ToList();
    }

    /// <inheritdoc />
    public ErrorOr<TransferIbanValidationResponse> ValidateRecipientIban(string iban)
    {
        bool isValid = Transfer.IsValidRecipientIban(iban);
        return new TransferIbanValidationResponse
        {
            IsValid = isValid,
            BankName = isValid ? Transfer.InferRecipientBankName(iban) : string.Empty
        };
    }

    /// <inheritdoc />
    public ErrorOr<TransferForexPreviewResponse> GetFxPreview(string sourceCurrency, string targetCurrency, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(sourceCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
        {
            return Error.Validation(description: "Both currencies are required.");
        }

        if (amount <= 0)
        {
            return Error.Validation(description: "Amount must be greater than zero.");
        }

        if (sourceCurrency.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new TransferForexPreviewResponse
            {
                ExchangeRate = 1m,
                ConvertedAmount = amount
            };
        }

        ErrorOr<decimal> rateResult = GetExchangeRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return rateResult.FirstError;
        }

        return new TransferForexPreviewResponse
        {
            ExchangeRate = rateResult.Value,
            ConvertedAmount = Math.Round(amount * rateResult.Value, 2)
        };
    }

    /// <inheritdoc />
    public bool RequiresTwoFactorAuthentication(decimal amount)
    {
        return Transfer.RequiresTwoFactorAuthentication(amount);
    }

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
            _logger.TransferAccountsLookupFailed(userId);
            return TransferErrors.AccountNotFound;
        }

        Account? account = accountsResult.Value.FirstOrDefault(account => account.Id == request.SourceAccountId);
        if (account is null)
        {
            _logger.TransferAccountNotFound(request.SourceAccountId, userId);
            return TransferErrors.AccountNotFound;
        }

        if (!account.IsActive())
        {
            _logger.TransferAccountNotActive(request.SourceAccountId);
            return TransferErrors.AccountNotActive;
        }

        if (!account.HasSufficientFunds(request.Amount))
        {
            _logger.TransferInsufficientFunds(request.SourceAccountId);
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
            _logger.TransferHistoryFetchFailed(userId);
            return result.FirstError;
        }

        return result.Value
            .Select(MapToResponse)
            .ToList();
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
            SourceAccountId = transfer.SourceAccount?.Id ?? 0,
            TransactionId = transfer.Transaction?.Id,
            TransactionRef = null,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            RecipientBankName = transfer.RecipientBankName,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            Fee = transfer.Fee,
            Reference = transfer.Reference,
            Status = transfer.Status,
            CreatedAt = transfer.CreatedAt
        };
    }

    private static ErrorOr<decimal> GetExchangeRate(string sourceCurrency, string targetCurrency)
    {
        Dictionary<string, decimal> rates = new()
        {
            { EurUsdPair, EurUsdRate },
            { EurGbpPair, EurGbpRate },
            { EurRonPair, EurRonRate },
            { UsdRonPair, UsdRonRate },
            { GbpRonPair, GbpRonRate }
        };

        string directPair = $"{sourceCurrency.ToUpperInvariant()}/{targetCurrency.ToUpperInvariant()}";
        if (rates.TryGetValue(directPair, out decimal directRate))
        {
            return directRate;
        }

        string inversePair = $"{targetCurrency.ToUpperInvariant()}/{sourceCurrency.ToUpperInvariant()}";
        if (rates.TryGetValue(inversePair, out decimal inverseRate))
        {
            return Math.Round(1 / inverseRate, ExchangeRatePrecision);
        }

        return Error.NotFound(description: $"Rate not found for pair {sourceCurrency}/{targetCurrency}.");
    }

    private static ErrorOr<Success> ValidateRequest(CreateTransferRequest request)
    {
        return Transfer.Validate(request.RecipientIban, request.Amount, request.Currency);
    }

    private ErrorOr<Success> CheckTwoFa(CreateTransferRequest request, int userId)
    {
        if (!Transfer.RequiresTwoFactorAuthentication(request.Amount))
        {
            return Result.Success;
        }

        if (string.IsNullOrWhiteSpace(request.TwoFaToken))
        {
            _logger.TransferTwoFactorMissing(request.Amount, userId);
            return TransferErrors.TwoFaRequired;
        }

        ErrorOr<bool> verifyResult = _otpService.VerifyTotp(userId, request.TwoFaToken);
        if (verifyResult.IsError || !verifyResult.Value)
        {
            _logger.TransferTwoFactorInvalid(userId);
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
            _logger.TransferDebitFailed(account.Id);
            return TransferErrors.DebitFailed;
        }

        decimal balanceAfter = account.Balance - request.Amount;

        var transaction = new Transaction
        {
            Account = account,
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
            CreatedAt = DateTime.UtcNow
        };

        ErrorOr<Transaction> logResult = _dashboardRepository.AddTransaction(transaction);
        if (logResult.IsError)
        {
            _logger.TransferTransactionLogFailed(account.Id);
            return TransferErrors.TransactionLogFailed;
        }

        var transfer = new Transfer
        {
            User = new User { Id = userId },
            SourceAccount = account,
            Transaction = logResult.Value,
            RecipientName = request.RecipientName,
            RecipientIban = request.RecipientIban,
            RecipientBankName = Transfer.InferRecipientBankName(request.RecipientIban),
            Amount = request.Amount,
            Currency = request.Currency,
            Fee = 0m,
            Reference = request.Reference,
            Status = TransferStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        ErrorOr<Transfer> persistResult = _dashboardRepository.AddTransfer(transfer);
        if (persistResult.IsError)
        {
            _logger.TransferPersistenceFailed(userId);
            return TransferErrors.PersistenceFailed;
        }

        Transfer persistedTransfer = persistResult.Value;
        return new TransferResponse
        {
            Id = persistedTransfer.Id,
            SourceAccountId = persistedTransfer.SourceAccount?.Id ?? 0,
            TransactionId = persistedTransfer.Transaction?.Id,
            TransactionRef = logResult.Value.TransactionRef,
            RecipientName = persistedTransfer.RecipientName,
            RecipientIban = persistedTransfer.RecipientIban,
            RecipientBankName = persistedTransfer.RecipientBankName,
            Amount = persistedTransfer.Amount,
            Currency = persistedTransfer.Currency,
            Fee = persistedTransfer.Fee,
            Reference = persistedTransfer.Reference,
            Status = persistedTransfer.Status,
            CreatedAt = persistedTransfer.CreatedAt
        };
    }
}
