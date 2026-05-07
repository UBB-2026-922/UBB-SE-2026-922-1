namespace BankingApp.Infrastructure.DataAccess.Interfaces;

using Domain.Aggregates.BillerAggregate;
using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines low-level data access operations for saved billers.
/// </summary>
public interface ISavedBillerDataAccess
{
    /// <summary>Returns all saved billers for a user, with the Biller navigation property populated.</summary>
    public ErrorOr<List<SavedBiller>> FindByUserId(int userId);

    /// <summary>Persists a new saved biller and returns it with the generated Id.</summary>
    public ErrorOr<SavedBiller> Add(SavedBiller savedBiller);

    /// <summary>Deletes a saved biller entry by its identifier.</summary>
    public ErrorOr<Success> Delete(int id);
}
