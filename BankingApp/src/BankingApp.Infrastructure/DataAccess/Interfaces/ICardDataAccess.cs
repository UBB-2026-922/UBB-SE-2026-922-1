namespace BankingApp.Infrastructure.DataAccess.Interfaces;

using Domain.Entities;
using ErrorOr;

/// <summary>
///     Defines data access operations for payment cards.
/// </summary>
public interface ICardDataAccess
{
    /// <summary>Finds all cards belonging to the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of cards owned by the user, or an error if the operation failed.</returns>
    public ErrorOr<List<Card>> FindByUserId(int userId);

    /// <summary>Finds a card by its unique identifier.</summary>
    /// <param name="id">The card identifier.</param>
    /// <returns>The matching <see cref="Card" />, or <see cref="Error.NotFound" /> if not found.</returns>
    public ErrorOr<Card> FindById(int id);
}
