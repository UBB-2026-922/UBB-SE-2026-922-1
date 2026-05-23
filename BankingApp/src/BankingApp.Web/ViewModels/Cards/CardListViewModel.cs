namespace BankingApp.Web.ViewModels.Cards;

/// <summary>View model for the cards list page (GET /Cards).</summary>
public class CardListViewModel
{
    /// <summary>Gets or sets the list of cards belonging to the current user.</summary>
    public List<CardRowViewModel> Cards { get; set; } = [];

    /// <summary>Gets a value indicating whether the user has any cards.</summary>
    public bool HasCards => Cards.Count > 0;
}
