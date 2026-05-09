namespace BankingApp.Domain.Entities;

using System;

/// <summary>
/// Represents a biller that a user has saved to their personal quick-pay list.
/// </summary>
/// <remarks>
///     Reuses the base entities <see cref="User" /> and <see cref="Biller" />
///     to model each saved quick-pay entry.
/// </remarks>
public class SavedBiller
{
    /// <summary>
    /// Gets or sets the unique identifier for this saved-biller entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this saved biller.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the biller associated with this saved record.
    /// </summary>
    public Biller? Biller { get; set; }

    /// <summary>
    /// Gets or sets the user-defined nickname for this saved biller.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the pre-filled reference number used when paying this biller.
    /// </summary>
    public string? DefaultReference { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) when this entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
