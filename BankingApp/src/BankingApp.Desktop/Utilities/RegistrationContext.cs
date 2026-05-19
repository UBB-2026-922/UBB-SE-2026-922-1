namespace BankingApp.Desktop.Utilities;

/// <inheritdoc />
public sealed class RegistrationContext : IRegistrationContext
{
    /// <summary>Gets or sets a value indicating whether the user was just registered.</summary>
    public bool JustRegistered { get; set; }
}
