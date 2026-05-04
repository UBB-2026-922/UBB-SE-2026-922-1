using System.Text.RegularExpressions;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Beneficiary;

/// <summary>
///     Provides business operations for managing beneficiaries.
/// </summary>
public class BeneficiaryService : IBeneficiaryService
{
    private readonly ILogger<BeneficiaryService> _logger;
    private readonly IBeneficiaryRepository _beneficiaryRepository;

    /// <summary>
    ///     Initializes a new instance of the BeneficiaryService class.
    /// </summary>
    /// <param name="beneficiaryRepository">The beneficiary repository.</param>
    /// <param name="logger">The logger.</param>
    public BeneficiaryService(
        IBeneficiaryRepository beneficiaryRepository,
        ILogger<BeneficiaryService> logger)
    {
        _beneficiaryRepository = beneficiaryRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<List<Beneficiary>> GetByUserId(int userId)
    {
        return _beneficiaryRepository.FindByUserId(userId);
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> GetById(int beneficiaryId)
    {
        return _beneficiaryRepository.FindById(beneficiaryId);
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Create(int userId, string name, string iban, string? bankName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation(
                code: "Beneficiary.NameRequired",
                description: "Beneficiary name cannot be empty.");
        }

        if (!ValidateIban(iban))
        {
            return Error.Validation(
                code: "Beneficiary.InvalidIban",
                description: "Invalid IBAN format.");
        }

        string normalizedName = name.Trim();
        string normalizedIban = iban.Trim().ToUpperInvariant();
        string? normalizedBankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim();

        ErrorOr<bool> existsResult = _beneficiaryRepository.ExistsByUserIdAndIban(userId, normalizedIban);
        if (existsResult.IsError)
        {
            _logger.LogError(
                "Failed to check beneficiary duplicate for user {UserId}.",
                userId);
            return existsResult.FirstError;
        }

        if (existsResult.Value)
        {
            return Error.Conflict(
                code: "Beneficiary.DuplicateIban",
                description: "A beneficiary with this IBAN already exists for this user.");
        }

        var beneficiary = new Beneficiary
        {
            UserId = userId,
            Name = normalizedName,
            Iban = normalizedIban,
            BankName = normalizedBankName,
            CreatedAt = DateTime.UtcNow,
            TotalAmountSent = 0,
            TransferCount = 0,
        };

        ErrorOr<Beneficiary> createResult = _beneficiaryRepository.Create(beneficiary);
        if (createResult.IsError)
        {
            _logger.LogError(
                "Failed to create beneficiary for user {UserId}.",
                userId);
            return createResult.FirstError;
        }

        _logger.LogInformation(
            "Beneficiary {BeneficiaryId} created for user {UserId}.",
            createResult.Value.Id,
            userId);

        return createResult.Value;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Name))
        {
            return Error.Validation(
                code: "Beneficiary.NameRequired",
                description: "Beneficiary name cannot be empty.");
        }

        if (!ValidateIban(beneficiary.Iban))
        {
            return Error.Validation(
                code: "Beneficiary.InvalidIban",
                description: "Invalid IBAN format.");
        }

        beneficiary.Name = beneficiary.Name.Trim();
        beneficiary.Iban = beneficiary.Iban.Trim().ToUpperInvariant();
        beneficiary.BankName = string.IsNullOrWhiteSpace(beneficiary.BankName)
            ? null
            : beneficiary.BankName.Trim();

        ErrorOr<bool> existsResult =
            _beneficiaryRepository.ExistsByUserIdAndIban(beneficiary.UserId, beneficiary.Iban);

        if (existsResult.IsError)
        {
            _logger.LogError(
                "Failed to check duplicate beneficiary during update for beneficiary {BeneficiaryId}.",
                beneficiary.Id);
            return existsResult.FirstError;
        }

        ErrorOr<List<Beneficiary>> userBeneficiariesResult =
            _beneficiaryRepository.FindByUserId(beneficiary.UserId);

        if (userBeneficiariesResult.IsError)
        {
            _logger.LogError(
                "Failed to load beneficiaries for user {UserId} during update.",
                beneficiary.UserId);
            return userBeneficiariesResult.FirstError;
        }

        bool duplicateOwnedByAnotherBeneficiary = userBeneficiariesResult.Value.Any(existingBeneficiary =>
            existingBeneficiary.Id != beneficiary.Id &&
            string.Equals(existingBeneficiary.Iban, beneficiary.Iban, StringComparison.OrdinalIgnoreCase));

        if (duplicateOwnedByAnotherBeneficiary)
        {
            return Error.Conflict(
                code: "Beneficiary.DuplicateIban",
                description: "A beneficiary with this IBAN already exists for this user.");
        }

        return _beneficiaryRepository.Update(beneficiary);
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int beneficiaryId)
    {
        return _beneficiaryRepository.Delete(beneficiaryId);
    }

    /// <inheritdoc />
    public bool ValidateIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        string normalized = iban.Replace(" ", string.Empty).Trim().ToUpperInvariant();

        if (normalized.Length < 15 || normalized.Length > 34)
        {
            return false;
        }

        return Regex.IsMatch(normalized, "^[A-Z]{2}[0-9]{2}[A-Z0-9]+$");
    }
}
