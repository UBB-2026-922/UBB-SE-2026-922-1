// <copyright file="Card.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Card class.
// </summary>

using BankApp.Domain.Enums;

namespace BankApp.Domain.Entities;

/// <summary>
///     Represents a payment card linked to a bank account.
/// </summary>
public class Card
{
    private const int VisibleSuffixLength = 4;
    private const string MaskedPrefix = "**** **** ****";
    private const string FullyMasked = "**** **** **** ****";

    /// <summary>
    ///     Gets or sets the unique identifier for the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the account this card belongs to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int AccountId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who owns this card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the card number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the cardholder.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the expiry date of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    ///     Gets or sets the card verification value.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Cvv { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public CardType CardType { get; set; }

    /// <summary>
    ///     Gets or sets the brand of the card (e.g. Visa, Mastercard).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? CardBrand { get; set; }

    /// <summary>
    ///     Gets or sets the status of the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public CardStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the daily transaction limit.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? DailyTransactionLimit { get; set; }

    /// <summary>
    ///     Gets or sets the monthly spending cap.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? MonthlySpendingCap { get; set; }

    /// <summary>
    ///     Gets or sets the ATM withdrawal limit.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? AtmWithdrawalLimit { get; set; }

    /// <summary>
    ///     Gets or sets the contactless payment limit.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? ContactlessLimit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether contactless payments are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsContactlessEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether online payments are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsOnlineEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets the display sort order for the card.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the card was cancelled, if applicable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the card was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Returns the card number with all but the last four digits replaced by asterisks.
    /// </summary>
    /// <returns>The masked card number.</returns>
    public string GetMaskedNumber()
    {
        if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < VisibleSuffixLength)
        {
            return FullyMasked;
        }

        return $"{MaskedPrefix} {CardNumber[^VisibleSuffixLength..]}";
    }

    /// <summary>
    ///     Returns true when the card's expiry date is in the past.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public bool IsExpired()
    {
        return ExpiryDate < DateTime.UtcNow;
    }

    /// <summary>
    ///     Returns true when the card status is Active.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public bool IsActive()
    {
        return Status == CardStatus.Active;
    }
}