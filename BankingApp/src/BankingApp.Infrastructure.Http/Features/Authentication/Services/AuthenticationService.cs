namespace BankingApp.Infrastructure.Http.Features.Authentication.Services;

using Application.Features.Authentication.Services;
using Application.Shared.Http;
using Contracts.Features.Authentication.Dtos;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using ErrorOr;

public sealed class AuthenticationService(IApiClient apiClient) : IAuthenticationService
{
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Auth.LoginFull, request, ct);

    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.RegisterFull, request, ct);

    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.Auth.VerifyOtpFull, request, ct);

    public Task<ErrorOr<Success>> ResendOtpAsync(int userId, CancellationToken ct = default)
        => apiClient.PostAsync($"{ApiEndpoints.Auth.ResendOtpFull}?userId={userId}", new { }, ct);

    public Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.ForgotPasswordFull, request, ct);

    public Task<ErrorOr<Success>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken ct = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.VerifyResetTokenFull, request, ct);

    public Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.ResetPasswordFull, request, ct);
}
