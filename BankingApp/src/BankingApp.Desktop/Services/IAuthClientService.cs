namespace BankingApp.Desktop.Services;

using System.Threading.Tasks;
using BankingApp.Application.Features.Authentication.Dtos;
using ErrorOr;

/// <summary>
///     Defines the desktop authentication client boundary used by login, registration,
///     and two-factor workflows.
/// </summary>
public interface IAuthClientService
{
    /// <summary>
    ///     Gets or sets the authenticated user identifier cached by the client.
    /// </summary>
    public int? CurrentUserId { get; set; }

    /// <summary>
    ///     Verifies whether the underlying API client is configured well enough to issue requests.
    /// </summary>
    public ErrorOr<Success> EnsureConfigured();

    /// <summary>
    ///     Authenticates a user with email and password credentials.
    /// </summary>
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password);

    /// <summary>
    ///     Stores the bearer token returned by the API.
    /// </summary>
    public void SetToken(string token);

    /// <summary>
    ///     Registers a new user account.
    /// </summary>
    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName);

    /// <summary>
    ///     Verifies an OTP during the two-factor flow.
    /// </summary>
    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode);

    /// <summary>
    ///     Requests a new OTP for the specified user.
    /// </summary>
    public Task<ErrorOr<object>> ResendOtpAsync(int userId);
}
