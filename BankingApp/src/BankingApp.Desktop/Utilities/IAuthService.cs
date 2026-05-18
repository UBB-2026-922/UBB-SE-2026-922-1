namespace BankingApp.Desktop.Utilities;

using BankingApp.Application.Common.Http;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using ErrorOr;
using System.Threading.Tasks;

/// <summary>
///     Coordinates authentication state and auth-related HTTP calls for the desktop client.
/// </summary>
public interface IAuthService
{
    /// <summary>Gets or sets the current authenticated user identifier.</summary>
    public int? CurrentUserId { get; set; }

    /// <summary>Checks whether the underlying HTTP client has a usable configuration.</summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>Logs the user in with email and password.</summary>
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password);

    /// <summary>Stores the bearer token used for authenticated requests.</summary>
    public void SetToken(string token);

    /// <summary>Clears any authenticated state.</summary>
    public void ClearToken();

    /// <summary>Registers a new user account.</summary>
    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName);

    /// <summary>Verifies a two-factor authentication code.</summary>
    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode);

    /// <summary>Requests a new two-factor authentication code.</summary>
    public Task<ErrorOr<object>> ResendOtpAsync(int userId);
}
