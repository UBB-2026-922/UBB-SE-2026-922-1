namespace BankingApp.Application.Services.Beneficiary;

using System.Text.RegularExpressions;
using Logging;
using Repositories.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Domain.Entities;

/// <summary>
///     Provides business operations for managing beneficiaries.
/// </summary>
public partial class BeneficiaryService : IBeneficiaryService
{
    private readonly ILogger<BeneficiaryService> _logger;
    private readonly IBeneficiaryRepository _beneficiaryRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiaryService" /> class.
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
    public ErrorOr<Beneficiary> GetById(int beneficiaryId, int userId)
    {
        return _beneficiaryRepository.FindById(beneficiaryId, userId);
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Create(int userId, string name, string iban, string? bankName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation(
                "Beneficiary.NameRequired",
                "Beneficiary name cannot be empty.");
        }

        if (!ValidateIban(iban))
        {
            return Error.Validation(
                "Beneficiary.InvalidIban",
                "Invalid IBAN format.");
        }

        string normalizedName = name.Trim();
        string normalizedIban = iban.Trim().ToUpperInvariant();
        string? normalizedBankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim();

        ErrorOr<bool> existsResult = _beneficiaryRepository.ExistsByUserIdAndIban(userId, normalizedIban);
        if (existsResult.IsError)
        {
            _logger.BeneficiaryDuplicateCheckFailed(userId);
            return existsResult.FirstError;
        }

        if (existsResult.Value)
        {
            return Error.Conflict(
                "Beneficiary.DuplicateIban",
                "A beneficiary with this IBAN already exists for this user.");
        }

        var beneficiary = new Beneficiary
        {
            UserId = userId,
            Name = normalizedName,
            Iban = normalizedIban,
            BankName = normalizedBankName,
            CreatedAt = DateTime.UtcNow,
            TotalAmountSent = 0,
            TransferCount = 0
        };

        ErrorOr<Beneficiary> createResult = _beneficiaryRepository.Create(beneficiary);
        if (createResult.IsError)
        {
            _logger.BeneficiaryCreateFailed(userId);
            return createResult.FirstError;
        }

        _logger.BeneficiaryCreated(createResult.Value.Id, userId);

        return createResult.Value;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Name))
        {
            return Error.Validation(
                "Beneficiary.NameRequired",
                "Beneficiary name cannot be empty.");
        }

        if (!ValidateIban(beneficiary.Iban))
        {
            return Error.Validation(
                "Beneficiary.InvalidIban",
                "Invalid IBAN format.");
        }

        string normalizedName = beneficiary.Name.Trim();
        string normalizedIban = beneficiary.Iban.Trim().ToUpperInvariant();
        string? normalizedBankName = string.IsNullOrWhiteSpace(beneficiary.BankName)
            ? null
            : beneficiary.BankName.Trim();

        ErrorOr<Beneficiary> existingBeneficiaryResult =
            _beneficiaryRepository.FindById(beneficiary.Id, beneficiary.UserId);
        if (existingBeneficiaryResult.IsError)
        {
            return existingBeneficiaryResult.FirstError;
        }

        ErrorOr<List<Beneficiary>> userBeneficiariesResult =
            _beneficiaryRepository.FindByUserId(beneficiary.UserId);

        if (userBeneficiariesResult.IsError)
        {
            _logger.BeneficiariesLoadForUpdateFailed(beneficiary.UserId);
            return userBeneficiariesResult.FirstError;
        }

        bool duplicateOwnedByAnotherBeneficiary = userBeneficiariesResult.Value.Any(existingBeneficiary =>
            existingBeneficiary.Id != beneficiary.Id &&
            string.Equals(existingBeneficiary.Iban, normalizedIban, StringComparison.OrdinalIgnoreCase));

        if (duplicateOwnedByAnotherBeneficiary)
        {
            return Error.Conflict(
                "Beneficiary.DuplicateIban",
                "A beneficiary with this IBAN already exists for this user.");
        }

        Beneficiary existingBeneficiary = existingBeneficiaryResult.Value;
        existingBeneficiary.Name = normalizedName;
        existingBeneficiary.Iban = normalizedIban;
        existingBeneficiary.BankName = normalizedBankName;

        return _beneficiaryRepository.Update(existingBeneficiary);
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int beneficiaryId, int userId)
    {
        return _beneficiaryRepository.Delete(beneficiaryId, userId);
    }

    /// <inheritdoc />
    public bool ValidateIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        string normalized = iban.Replace(" ", string.Empty).Trim().ToUpperInvariant();

        return normalized.Length is >= 15 and <= 34 && MyRegex().IsMatch(normalized);
    }

    [GeneratedRegex("^[A-Z]{2}[0-9]{2}[A-Z0-9]+$")]
    private static partial Regex MyRegex();
}
