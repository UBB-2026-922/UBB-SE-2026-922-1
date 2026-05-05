// <copyright file="BeneficiaryService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryService class.
// </summary>

using System.Text.RegularExpressions;
using BankingApp.Application.Repositories.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;
using DomainBeneficiary = BankingApp.Domain.Entities.Beneficiary;

namespace BankingApp.Application.Services.Beneficiary;

/// <summary>
///     Provides business operations for managing beneficiaries.
/// </summary>
public class BeneficiaryService : IBeneficiaryService
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
    public ErrorOr<List<DomainBeneficiary>> GetByUserId(int userId)
    {
        return _beneficiaryRepository.FindByUserId(userId);
    }

    /// <inheritdoc />
    public ErrorOr<DomainBeneficiary> GetById(int beneficiaryId, int userId)
    {
        return _beneficiaryRepository.FindById(beneficiaryId, userId);
    }

    /// <inheritdoc />
    public ErrorOr<DomainBeneficiary> Create(int userId, string name, string iban, string? bankName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation(
                "Beneficiary.NameRequired",
                "Beneficiary name cannot be empty.");

        if (!ValidateIban(iban))
            return Error.Validation(
                "Beneficiary.InvalidIban",
                "Invalid IBAN format.");

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
            return Error.Conflict(
                "Beneficiary.DuplicateIban",
                "A beneficiary with this IBAN already exists for this user.");

        var beneficiary = new DomainBeneficiary
        {
            UserId = userId,
            Name = normalizedName,
            Iban = normalizedIban,
            BankName = normalizedBankName,
            CreatedAt = DateTime.UtcNow,
            TotalAmountSent = 0,
            TransferCount = 0
        };

        ErrorOr<DomainBeneficiary> createResult = _beneficiaryRepository.Create(beneficiary);
        if (createResult.IsError)
        {
            _logger.LogError(
                "Failed to create beneficiary for user {UserId}.",
                userId);
            return createResult.FirstError;
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Beneficiary {BeneficiaryId} created for user {UserId}.",
                createResult.Value.Id,
                userId);
        }

        return createResult.Value;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(DomainBeneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Name))
            return Error.Validation(
                "Beneficiary.NameRequired",
                "Beneficiary name cannot be empty.");

        if (!ValidateIban(beneficiary.Iban))
            return Error.Validation(
                "Beneficiary.InvalidIban",
                "Invalid IBAN format.");

        string normalizedName = beneficiary.Name.Trim();
        string normalizedIban = beneficiary.Iban.Trim().ToUpperInvariant();
        string? normalizedBankName = string.IsNullOrWhiteSpace(beneficiary.BankName)
            ? null
            : beneficiary.BankName.Trim();

        ErrorOr<DomainBeneficiary> existingBeneficiaryResult =
            _beneficiaryRepository.FindById(beneficiary.Id, beneficiary.UserId);
        if (existingBeneficiaryResult.IsError) return existingBeneficiaryResult.FirstError;

        ErrorOr<List<DomainBeneficiary>> userBeneficiariesResult =
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
            string.Equals(existingBeneficiary.Iban, normalizedIban, StringComparison.OrdinalIgnoreCase));

        if (duplicateOwnedByAnotherBeneficiary)
            return Error.Conflict(
                "Beneficiary.DuplicateIban",
                "A beneficiary with this IBAN already exists for this user.");

        DomainBeneficiary existingBeneficiary = existingBeneficiaryResult.Value;
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
        if (string.IsNullOrWhiteSpace(iban)) return false;

        string normalized = iban.Replace(" ", string.Empty).Trim().ToUpperInvariant();

        if (normalized.Length < 15 || normalized.Length > 34) return false;

        return Regex.IsMatch(normalized, "^[A-Z]{2}[0-9]{2}[A-Z0-9]+$");
    }
}
