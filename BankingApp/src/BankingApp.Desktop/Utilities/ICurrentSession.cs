namespace BankingApp.Desktop.Utilities;

using ErrorOr;

/// <summary>
///     Represents the local authentication session state for the currently signed-in desktop user.
///     Implemented by <see cref="ApiClient" />, which holds the token and user-id after a successful login.
/// </summary>
public interface ICurrentSession
{
    /// <summary>Gets or sets the identifier of the currently authenticated user.</summary>
    public int? CurrentUserId { get; set; }

    /// <summary>Gets the currently configured bearer token.</summary>
    public string? Token { get; }

    /// <summary>Returns <see cref="Success" /> when the underlying HTTP client is correctly configured.</summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>Gets the identifier of the currently authenticated user.</summary>
    public int? GetCurrentUserId();

    /// <summary>Sets the identifier of the currently authenticated user.</summary>
    public void SetCurrentUserId(int userId);

    /// <summary>Sets the bearer token used for authenticated requests.</summary>
    public void SetToken(string tokenStr);

    /// <summary>Clears all stored authentication state.</summary>
    public void ClearToken();
}
