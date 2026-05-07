namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Interfaces;
using ErrorOr;

/// <summary>
///     Provides SQL Server data access for payment card records.
/// </summary>
public class CardDataAccess : ICardDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CardDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public CardDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Card> FindById(int id)
    {
        Card? card = _databaseContext.Cards.FirstOrDefault(c => c.Id == id);
        if (card == null)
        {
            return Error.NotFound(description: "Card not found.");
        }

        return card;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Card>> FindByUserId(int userId)
    {
        var cards = _databaseContext.Cards.Where(card => card.UserId == userId).ToList();
        return cards;
    }
}
