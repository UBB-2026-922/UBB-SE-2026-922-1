namespace BankingApp.Desktop.Services.Transfers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Beneficiaries;
using Application.DTOs.Transfer;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;

/// <summary>
///     Desktop transfer service that keeps orchestration in the client and uses proxy repositories for data access.
/// </summary>
internal sealed class TransferService : ITransferService
{
    private const int ExchangeRatePrecision = 4;
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

    private readonly IApiClient _apiClient;
    private readonly IBeneficiaryRepository _beneficiaryRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public TransferService(
        IApiClient apiClient,
        IDashboardRepository dashboardRepository,
        IBeneficiaryRepository beneficiaryRepository)
    {
        _apiClient = apiClient;
        _dashboardRepository = dashboardRepository;
        _beneficiaryRepository = beneficiaryRepository;
    }

    public Task<ErrorOr<List<TransferAccountSelectionResponse>>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<TransferAccountSelectionResponse>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<Account>> result = _dashboardRepository.GetAccountsByUser(userId.Value);
        if (result.IsError)
        {
            return Task.FromResult<ErrorOr<List<TransferAccountSelectionResponse>>>(result.FirstError);
        }

        var accounts = result.Value
            .Where(account => account.Status == AccountStatus.Active)
            .OrderBy(account => account.AccountName)
            .Select(account => new TransferAccountSelectionResponse
            {
                Id = account.Id,
                Iban = account.Iban,
                Currency = account.Currency,
                Balance = account.Balance,
                AccountName = account.AccountName ?? string.Empty,
            })
            .ToList();

        return Task.FromResult<ErrorOr<List<TransferAccountSelectionResponse>>>(accounts);
    }

    public Task<ErrorOr<TransferExecutionResponse>> ExecuteTransferAsync(
        int sourceAccountId,
        string recipientName,
        string recipientIban,
        decimal amount,
        string currency,
        string? twoFaToken)
    {
        var request = new CreateTransferRequest
        {
            SourceAccountId = sourceAccountId,
            RecipientName = recipientName,
            RecipientIban = recipientIban,
            Amount = amount,
            Currency = currency,
            TwoFaToken = twoFaToken,
        };

        return _apiClient.PostAsync<CreateTransferRequest, TransferExecutionResponse>(ApiEndpoints.TransferExecute, request);
    }

    public Task<ErrorOr<TransferIbanValidationResponse>> ValidateIbanAsync(string iban)
    {
        bool isValid = Transfer.IsValidRecipientIban(iban);
        return Task.FromResult<ErrorOr<TransferIbanValidationResponse>>(new TransferIbanValidationResponse
        {
            IsValid = isValid,
            BankName = isValid ? Transfer.InferRecipientBankName(iban) : string.Empty,
        });
    }

    public Task<ErrorOr<TransferForexPreviewResponse>> GetFxPreviewAsync(
        string fromCurrency,
        string toCurrency,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(Error.Validation(description: "Both currencies are required."));
        }

        if (amount <= 0)
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(Error.Validation(description: "Amount must be greater than zero."));
        }

        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(new TransferForexPreviewResponse
            {
                ExchangeRate = 1m,
                ConvertedAmount = amount,
            });
        }

        ErrorOr<decimal> rateResult = GetExchangeRate(fromCurrency, toCurrency);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(rateResult.FirstError);
        }

        return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(new TransferForexPreviewResponse
        {
            ExchangeRate = rateResult.Value,
            ConvertedAmount = Math.Round(amount * rateResult.Value, 2),
        });
    }

    public Task<ErrorOr<List<TransferResponse>>> GetTransferHistoryAsync(CancellationToken cancellationToken = default)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<TransferResponse>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<Transfer>> result = _dashboardRepository.GetTransfersByUserId(userId.Value);
        if (result.IsError)
        {
            return Task.FromResult<ErrorOr<List<TransferResponse>>>(result.FirstError);
        }

        var mapped = result.Value.Select(transfer => new TransferResponse
        {
            Id = transfer.Id,
            SourceAccountId = transfer.SourceAccount?.Id ?? 0,
            TransactionId = transfer.Transaction?.Id,
            TransactionRef = transfer.Transaction?.TransactionRef,
            RecipientName = transfer.RecipientName,
            RecipientIban = transfer.RecipientIban,
            RecipientBankName = transfer.RecipientBankName,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            Fee = transfer.Fee,
            Reference = transfer.Reference,
            Status = transfer.Status,
            CreatedAt = transfer.CreatedAt,
        }).ToList();

        return Task.FromResult<ErrorOr<List<TransferResponse>>>(mapped);
    }

    public Task<ErrorOr<List<BeneficiaryDto>>> GetBeneficiariesAsync(CancellationToken cancellationToken = default)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<List<BeneficiaryDto>>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        ErrorOr<List<Beneficiary>> result = _beneficiaryRepository.FindByUserId(userId.Value);
        if (result.IsError)
        {
            return Task.FromResult<ErrorOr<List<BeneficiaryDto>>>(result.FirstError);
        }

        var beneficiaries = result.Value.Select(beneficiary => new BeneficiaryDto
        {
            Id = beneficiary.Id,
            Name = beneficiary.Name,
            Iban = beneficiary.Iban,
            BankName = beneficiary.BankName,
            LastTransferDate = beneficiary.LastTransferDate,
            TotalAmountSent = beneficiary.TotalAmountSent,
            TransferCount = beneficiary.TransferCount,
            CreatedAt = beneficiary.CreatedAt,
        }).ToList();

        return Task.FromResult<ErrorOr<List<BeneficiaryDto>>>(beneficiaries);
    }

    public Task<ErrorOr<Success>> AddBeneficiaryAsync(string name, string iban, string bankName)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Validation("Beneficiary.NameRequired", "Beneficiary name cannot be empty."));
        }

        if (!ValidateBeneficiaryIban(iban))
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Validation("Beneficiary.InvalidIban", "Invalid IBAN format."));
        }

        string normalizedIban = iban.Trim().ToUpperInvariant();
        ErrorOr<bool> existsResult = _beneficiaryRepository.ExistsByUserIdAndIban(userId.Value, normalizedIban);
        if (existsResult.IsError)
        {
            return Task.FromResult<ErrorOr<Success>>(existsResult.FirstError);
        }

        if (existsResult.Value)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Conflict("Beneficiary.DuplicateIban", "A beneficiary with this IBAN already exists for this user."));
        }

        ErrorOr<Beneficiary> createResult = _beneficiaryRepository.Create(new Beneficiary
        {
            User = new User { Id = userId.Value },
            Name = name.Trim(),
            Iban = normalizedIban,
            BankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim(),
            CreatedAt = DateTime.UtcNow,
            TotalAmountSent = 0,
            TransferCount = 0,
        });

        return Task.FromResult<ErrorOr<Success>>(createResult.IsError ? createResult.FirstError : Result.Success);
    }

    public Task<ErrorOr<Success>> DeleteBeneficiaryAsync(int beneficiaryId)
    {
        int? userId = _apiClient.GetCurrentUserId();
        if (userId is null)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Unauthorized(description: "User is not authenticated."));
        }

        return Task.FromResult(_beneficiaryRepository.Delete(beneficiaryId, userId.Value));
    }

    private static bool ValidateBeneficiaryIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        string normalized = iban.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();
        return normalized.Length is >= 15 and <= 34
               && normalized.Length >= 4
               && char.IsLetter(normalized[0])
               && char.IsLetter(normalized[1])
               && char.IsDigit(normalized[2])
               && char.IsDigit(normalized[3]);
    }

    private static ErrorOr<decimal> GetExchangeRate(string sourceCurrency, string targetCurrency)
    {
        Dictionary<string, decimal> rates = new()
        {
            { EurUsdPair, EurUsdRate },
            { EurGbpPair, EurGbpRate },
            { EurRonPair, EurRonRate },
            { UsdRonPair, UsdRonRate },
            { GbpRonPair, GbpRonRate },
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
}
