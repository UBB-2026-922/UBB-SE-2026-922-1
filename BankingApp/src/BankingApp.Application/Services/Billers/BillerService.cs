// <copyright file="BillerService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillerService class.
// </summary>

using BankingApp.Application.DataTransferObjects.Billers;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using ErrorOr;

namespace BankingApp.Application.Services.Billers;

/// <summary>
///     Provides biller directory and saved-biller management use cases.
/// </summary>
public class BillerService : IBillerService
{
    private readonly IBillerRepository _billerRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerService" /> class.
    /// </summary>
    /// <param name="billerRepository">The biller repository.</param>
    public BillerService(IBillerRepository billerRepository)
    {
        _billerRepository = billerRepository;
    }

    /// <inheritdoc />
    public ErrorOr<List<BillerDataTransferObject>> GetBillerDirectory()
    {
        ErrorOr<List<Biller>> result = _billerRepository.GetAllBillers(activeOnly: true);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<List<BillerDataTransferObject>> SearchBillers(string searchTerm, string? category = null)
    {
        ErrorOr<List<Biller>> result = _billerRepository.SearchBillers(searchTerm, category, activeOnly: true);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<List<SavedBillerDataTransferObject>> GetSavedBillers(int userId)
    {
        ErrorOr<List<SavedBiller>> result = _billerRepository.GetSavedBillers(userId);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.Select(ToSavedDto).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<SavedBillerDataTransferObject> SaveBiller(int userId, SaveBillerRequest request)
    {
        ErrorOr<Biller> billerResult = _billerRepository.GetBillerById(request.BillerId);
        if (billerResult.IsError)
        {
            return BillerErrors.BillerNotFound;
        }

        ErrorOr<List<SavedBiller>> existingResult = _billerRepository.GetSavedBillers(userId);
        if (!existingResult.IsError && existingResult.Value.Any(s => s.BillerId == request.BillerId))
        {
            return BillerErrors.BillerAlreadySaved;
        }

        var savedBiller = new SavedBiller
        {
            UserId = userId,
            BillerId = request.BillerId,
            Nickname = request.Nickname,
            DefaultReference = request.DefaultReference,
            CreatedAt = DateTime.UtcNow,
        };

        ErrorOr<SavedBiller> saveResult = _billerRepository.SaveBiller(savedBiller);
        if (saveResult.IsError)
        {
            return saveResult.Errors;
        }

        saveResult.Value.Biller = billerResult.Value;
        return ToSavedDto(saveResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<Success> RemoveSavedBiller(int userId, int savedBillerId)
    {
        ErrorOr<List<SavedBiller>> savedResult = _billerRepository.GetSavedBillers(userId);
        if (savedResult.IsError)
        {
            return savedResult.Errors;
        }

        SavedBiller? entry = savedResult.Value.FirstOrDefault(s => s.Id == savedBillerId);
        if (entry is null)
        {
            return BillerErrors.SavedBillerNotFound;
        }

        return _billerRepository.DeleteSavedBiller(savedBillerId);
    }

    private static BillerDataTransferObject ToDto(Biller b) =>
        new()
        {
            Id = b.Id,
            Name = b.Name,
            Category = b.Category,
            LogoUrl = b.LogoUrl,
            IsActive = b.IsActive,
        };

    private static SavedBillerDataTransferObject ToSavedDto(SavedBiller s) =>
        new()
        {
            Id = s.Id,
            BillerId = s.BillerId,
            BillerName = s.Biller?.Name ?? string.Empty,
            BillerCategory = s.Biller?.Category ?? string.Empty,
            LogoUrl = s.Biller?.LogoUrl,
            Nickname = s.Nickname,
            DefaultReference = s.DefaultReference,
            CreatedAt = s.CreatedAt,
        };
}
