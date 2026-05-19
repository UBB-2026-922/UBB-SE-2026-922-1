namespace BankingApp.Desktop.Features.Registration;

/// <summary>Stores short-lived desktop registration flow state.</summary>
public interface IRegistrationContext
{
    /// <summary>Gets or sets a value indicating whether the user was just registered.</summary>
    public bool JustRegistered { get; set; }
}
