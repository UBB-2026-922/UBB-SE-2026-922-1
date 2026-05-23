namespace BankingApp.Web.ViewModels.Cards;

using BankingApp.Domain.Enums;

/// <summary>Represents a single card row in the cards list view.</summary>
public class CardRowViewModel
{
    /// <summary>Gets or sets the card identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the masked card number (last 4 digits visible).</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the full unmasked card number.</summary>
    public string FullCardNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the card security code (CVV).</summary>
    public string SecurityCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the cardholder.</summary>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>Gets or sets the card expiry date.</summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>Gets or sets the card type (Debit / Credit).</summary>
    public CardType CardType { get; set; }

    /// <summary>Gets or sets the card brand (e.g. Visa, Mastercard).</summary>
    public string? CardBrand { get; set; }

    /// <summary>Gets or sets the current card status.</summary>
    public CardStatus Status { get; set; }

    /// <summary>Gets or sets a value indicating whether contactless payments are enabled.</summary>
    public bool IsContactlessEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether online payments are enabled.</summary>
    public bool IsOnlineEnabled { get; set; }

    /// <summary>Gets or sets the display name of the associated account.</summary>
    public string? AccountName { get; set; }

    /// <summary>Gets the status badge CSS class.</summary>
    public string StatusBadgeClass => Status switch
    {
        CardStatus.Active => "bg-success",
        CardStatus.Frozen => "bg-warning text-dark",
        CardStatus.Cancelled => "bg-secondary",
        CardStatus.Expired => "bg-danger",
        _ => "bg-secondary"
    };

    /// <summary>Gets a value indicating whether the card can be frozen.</summary>
    public bool CanFreeze => Status == CardStatus.Active;

    /// <summary>Gets a value indicating whether the card can be unfrozen.</summary>
    public bool CanUnfreeze => Status == CardStatus.Frozen;

    /// <summary>Gets a value indicating whether the card can be cancelled.</summary>
    public bool CanCancel => Status == CardStatus.Active || Status == CardStatus.Frozen;
}
