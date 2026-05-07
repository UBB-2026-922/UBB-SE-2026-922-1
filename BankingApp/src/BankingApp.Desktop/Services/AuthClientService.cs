namespace BankingApp.Desktop.Services;

using System;
using System.Threading.Tasks;
using BankingApp.Application.Features.Authentication.Dtos;
using BankingApp.Application.Features.UserRegistration.Dtos;
using ErrorOr;

/// <summary>
///     Implements <see cref="IAuthClientService" /> using the shared desktop API client.
/// </summary>
internal sealed class AuthClientService : IAuthClientService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthClientService" /> class.
    /// </summary>
    public AuthClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public int? CurrentUserId
    {
        get => _apiClient.CurrentUserId;
        set => _apiClient.CurrentUserId = value;
    }

    /// <inheritdoc />
    public ErrorOr<Success> EnsureConfigured() => _apiClient.EnsureConfigured();

    /// <inheritdoc />
    public void SetToken(string token) => _apiClient.SetToken(token);

    /// <inheritdoc />
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        return _apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Login, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName)
    {
        var request = new RegisterRequest { Email = email, Password = password, FullName = fullName };
        return _apiClient.PostAsync(ApiEndpoints.Register, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode)
    {
        var request = new VerifyOtpRequest { UserId = userId, OtpCode = otpCode };
        return _apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.VerifyOtp, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<object>> ResendOtpAsync(int userId)
    {
        return _apiClient.PostAsync<object?, object>($"{ApiEndpoints.ResendOtp}?userId={userId}", null);
    }
}
