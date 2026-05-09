namespace BankingApp.Infrastructure.Repositories.Implementations;

using System.Collections.Generic;
using System.Linq;
using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using DataAccess;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides repository operations for managing beneficiaries.
/// </summary>
public class BeneficiaryRepository : IBeneficiaryRepository
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiaryRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context.</param>
    public BeneficiaryRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> FindById(int beneficiaryId, int userId)
    {
        Beneficiary? beneficiary = _databaseContext.Beneficiaries
            .AsNoTracking()
            .FirstOrDefault(beneficiary => beneficiary.Id == beneficiaryId && EF.Property<int>(beneficiary, "UserId") == userId);

        if (beneficiary is null)
        {
            return Error.NotFound(
                "Beneficiary.NotFound",
                $"Beneficiary with id '{beneficiaryId}' was not found.");
        }

        return beneficiary;
    }

    /// <inheritdoc />
    public ErrorOr<List<Beneficiary>> FindByUserId(int userId)
    {
        var beneficiaries = _databaseContext.Beneficiaries
            .AsNoTracking()
            .Where(beneficiary => EF.Property<int>(beneficiary, "UserId") == userId)
            .OrderBy(beneficiary => beneficiary.Name)
            .ToList();

        return beneficiaries;
    }

    /// <inheritdoc />
    public ErrorOr<bool> ExistsByUserIdAndIban(int userId, string iban)
    {
        string normalizedIban = iban.Trim().ToUpperInvariant();
        bool exists = _databaseContext.Beneficiaries
            .AsNoTracking()
            .Any(beneficiary =>
                EF.Property<int>(beneficiary, "UserId") == userId &&
                beneficiary.Iban == normalizedIban);

        return exists;
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Create(Beneficiary beneficiary)
    {
        try
        {
            _databaseContext.Beneficiaries.Add(beneficiary);
            _databaseContext.SaveChanges();
            return beneficiary;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                "Beneficiary.CreateFailed",
                $"Failed to create beneficiary: {databaseUpdateException.Message}");
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(Beneficiary beneficiary)
    {
        int userId = beneficiary.User?.Id ?? 0;
        Beneficiary? existingBeneficiary = _databaseContext.Beneficiaries
            .FirstOrDefault(currentBeneficiary =>
                currentBeneficiary.Id == beneficiary.Id &&
                EF.Property<int>(currentBeneficiary, "UserId") == userId);

        if (existingBeneficiary is null)
        {
            return Error.NotFound(
                "Beneficiary.NotFound",
                $"Beneficiary with id '{beneficiary.Id}' was not found.");
        }

        try
        {
            existingBeneficiary.Name = beneficiary.Name;
            existingBeneficiary.Iban = beneficiary.Iban;
            existingBeneficiary.BankName = beneficiary.BankName;
            existingBeneficiary.LastTransferDate = beneficiary.LastTransferDate;
            existingBeneficiary.TotalAmountSent = beneficiary.TotalAmountSent;
            existingBeneficiary.TransferCount = beneficiary.TransferCount;
            existingBeneficiary.CreatedAt = beneficiary.CreatedAt;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                "Beneficiary.UpdateFailed",
                $"Failed to update beneficiary: {databaseUpdateException.Message}");
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int beneficiaryId, int userId)
    {
        Beneficiary? beneficiary = _databaseContext.Beneficiaries
            .FirstOrDefault(existingBeneficiary =>
                existingBeneficiary.Id == beneficiaryId &&
                EF.Property<int>(existingBeneficiary, "UserId") == userId);

        if (beneficiary is null)
        {
            return Error.NotFound(
                "Beneficiary.NotFound",
                $"Beneficiary with id '{beneficiaryId}' was not found.");
        }

        try
        {
            _databaseContext.Beneficiaries.Remove(beneficiary);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                "Beneficiary.DeleteFailed",
                $"Failed to delete beneficiary: {databaseUpdateException.Message}");
        }
    }
}
