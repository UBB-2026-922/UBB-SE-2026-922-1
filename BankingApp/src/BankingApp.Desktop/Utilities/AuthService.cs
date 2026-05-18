namespace BankingApp.Desktop.Utilities;

using System;
using BankingApp.Application.Common.Http;
using Contracts.Features.Authentication.Dtos;
using Contracts.Http;
using Contracts.Features.UserRegistration.Dtos;
using ErrorOr;
using System.Threading.Tasks;

/// <inheritdoc />
public sealed class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;

    /// <summary>Initializes a new instance of the <see cref="AuthService"/> class.</summary>
    public AuthService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public int? CurrentUserId { get; set; }

    /// <inheritdoc />
    public ErrorOr<Success> EnsureConfigured() => _apiClient.EnsureConfigured();

    /// <inheritdoc />
    public void SetToken(string token) => _apiClient.SetToken(token);

    /// <inheritdoc />
    public void ClearToken()
    {
        CurrentUserId = null;
        _apiClient.ClearToken();
    }

    /// <inheritdoc />
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password)
    {
        LoginRequest request = new()
        {
            Email = email,
            Password = password,
        };

        return _apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Login, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName)
    {
        RegisterRequest request = new()
        {
            Email = email,
            Password = password,
            FullName = fullName,
        };

        return _apiClient.PostAsync(ApiEndpoints.Register, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode)
    {
        VerifyOtpRequest request = new()
        {
            UserId = userId,
            OtpCode = otpCode,
        };

        return _apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.VerifyOtp, request);
    }

    /// <inheritdoc />
    public Task<ErrorOr<object>> ResendOtpAsync(int userId)
        => _apiClient.PostAsync<object?, object>($"{ApiEndpoints.ResendOtp}?userId={userId}", null);
}
