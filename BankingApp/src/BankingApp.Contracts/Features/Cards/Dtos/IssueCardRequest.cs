namespace BankingApp.Contracts.Features.Cards.Dtos;

using System.Text.Json.Serialization;
using Domain.Enums;

/// <summary>Represents the data required to issue a new card. The bank generates all other details.</summary>
public sealed class IssueCardRequest
{
    /// <summary>Gets or sets the card type (Debit or Credit).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardType CardType { get; set; }

    /// <summary>Gets or sets the card carrier (e.g. Visa, Mastercard).</summary>
    public string? CardBrand { get; set; }
}
