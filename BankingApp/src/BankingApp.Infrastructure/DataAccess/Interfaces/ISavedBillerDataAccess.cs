// <copyright file="ISavedBillerDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ISavedBillerDataAccess interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
///     Defines low-level data access operations for saved billers.
/// </summary>
public interface ISavedBillerDataAccess
{
    /// <summary>Returns all saved billers for a user, with the Biller navigation property populated.</summary>
    ErrorOr<List<SavedBiller>> FindByUserId(int userId);

    /// <summary>Persists a new saved biller and returns it with the generated Id.</summary>
    ErrorOr<SavedBiller> Add(SavedBiller savedBiller);

    /// <summary>Deletes a saved biller entry by its identifier.</summary>
    ErrorOr<Success> Delete(int id);
}
