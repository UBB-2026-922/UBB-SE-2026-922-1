namespace BankingApp.Web.ViewModels.Cards;

using System.ComponentModel.DataAnnotations;
using BankingApp.Domain.Enums;

/// <summary>View model for the issue-card form.</summary>
public class IssueCardViewModel
{
    /// <summary>Gets or sets the card type (Debit or Credit).</summary>
    [Required(ErrorMessage = "Please select a card type.")]
    public CardType CardType { get; set; }

    /// <summary>Gets or sets the card brand (Visa or Mastercard).</summary>
    [Required(ErrorMessage = "Please select a card brand.")]
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>Gets the supported card brands.</summary>
    public static IReadOnlyList<string> SupportedBrands { get; } = ["Visa", "Mastercard"];
}
