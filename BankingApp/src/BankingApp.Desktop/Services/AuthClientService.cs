using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Auth;
using BankingApp.Desktop.Utilities;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public class AuthClientService : IAuthClientService
{
    private readonly IApiClient _apiClient;

    public AuthClientService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public int? CurrentUserId
    {
        get => _apiClient.CurrentUserId;
        set => _apiClient.CurrentUserId = value;
    }

    public ErrorOr<Success> EnsureConfigured() => _apiClient.EnsureConfigured();

    public void SetToken(string token) => _apiClient.SetToken(token);

    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        return _apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Login, request);
    }

    public Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName)
    {
        var request = new RegisterRequest { Email = email, Password = password, FullName = fullName };
        return _apiClient.PostAsync(ApiEndpoints.Register, request);
    }

    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode)
    {
        var request = new VerifyOtpRequest { UserId = userId, OtpCode = otpCode };
        return _apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.VerifyOtp, request);
    }

    public Task<ErrorOr<object>> ResendOtpAsync(int userId)
    {
        return _apiClient.PostAsync<object?, object>(
            $"{ApiEndpoints.ResendOtp}?userId={userId}",
            null);
    }
}
