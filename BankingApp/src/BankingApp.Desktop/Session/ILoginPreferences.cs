namespace BankingApp.Desktop.Session;

/// <summary>Persists user login preferences across application launches.</summary>
public interface ILoginPreferences
{
    /// <summary>Gets the previously remembered email address, if any.</summary>
    string? SavedEmail { get; }

    /// <summary>Gets a value indicating whether the user previously opted to be remembered.</summary>
    bool RememberMe { get; }

    /// <summary>Saves or clears the remembered email based on the user's preference.</summary>
    void Save(string email, bool rememberMe);

    /// <summary>Clears any saved login preferences.</summary>
    void Clear();
}
