// <copyright file="IBillerRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IBillerRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines repository operations for billers and saved billers.
/// </summary>
public interface IBillerRepository
{
    /// <summary>Returns all billers, optionally filtered to active-only.</summary>
    /// <param name="activeOnly">When <see langword="true" />, returns only active billers.</param>
    /// <returns>The list of matching billers.</returns>
    ErrorOr<List<Biller>> GetAllBillers(bool activeOnly = true);

    /// <summary>Searches billers by name and/or category.</summary>
    /// <param name="searchTerm">Text to match against biller names (case-insensitive).</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="activeOnly">When <see langword="true" />, restricts results to active billers.</param>
    /// <returns>The list of matching billers.</returns>
    ErrorOr<List<Biller>> SearchBillers(string searchTerm, string? category = null, bool activeOnly = true);

    /// <summary>Finds a biller by its unique identifier.</summary>
    /// <param name="billerId">The biller identifier.</param>
    /// <returns>The matching <see cref="Biller" />, or <see cref="ErrorOr" /> not-found.</returns>
    ErrorOr<Biller> GetBillerById(int billerId);

    /// <summary>Returns all saved billers for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The list of saved biller entries with navigation properties populated.</returns>
    ErrorOr<List<SavedBiller>> GetSavedBillers(int userId);

    /// <summary>Saves a biller for a user.</summary>
    /// <param name="savedBiller">The saved biller entity to persist.</param>
    /// <returns>The persisted <see cref="SavedBiller" /> with its generated Id, or an error.</returns>
    ErrorOr<SavedBiller> SaveBiller(SavedBiller savedBiller);

    /// <summary>Removes a saved biller entry by its identifier.</summary>
    /// <param name="savedBillerId">The saved biller entry identifier.</param>
    /// <returns>Success, or not-found/failure error.</returns>
    ErrorOr<Success> DeleteSavedBiller(int savedBillerId);
}