namespace BankingApp.Desktop.Utilities;

/// <summary>Carries transient state across the register-to-login navigation boundary.</summary>
public interface IRegistrationContext
{
    /// <summary>Gets or sets a value indicating whether the user has just completed registration.</summary>
    public bool JustRegistered { get; set; }
}
