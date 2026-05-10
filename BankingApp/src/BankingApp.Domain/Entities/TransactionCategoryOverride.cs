namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a user override of the default category for a transaction.
/// </summary>
public class TransactionCategoryOverride
{
    /// <summary>
    ///     Gets or sets the unique identifier for the override.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the transaction being overridden.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <summary>
    ///     Gets or sets the user who created this override.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    ///     Gets or sets the category assigned by the user.
    /// </summary>
    public Category? Category { get; set; }
}
