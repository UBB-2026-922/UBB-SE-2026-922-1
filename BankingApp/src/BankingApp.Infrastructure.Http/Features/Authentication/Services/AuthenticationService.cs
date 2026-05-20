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
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Auth.LoginFull, request, cancellationToken);

    public Task<ErrorOr<Success>> LogoutAsync(CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.LogoutFull, new { }, cancellationToken);

    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.RegisterFull, request, cancellationToken);

    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.Auth.VerifyOtpFull, request, cancellationToken);

    public Task<ErrorOr<Success>> ResendOtpAsync(int userId, CancellationToken cancellationToken = default)
        => apiClient.PostAsync($"{ApiEndpoints.Auth.ResendOtpFull}?userId={userId}", new { }, cancellationToken);

    public Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.ForgotPasswordFull, request, cancellationToken);

    public Task<ErrorOr<Success>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.VerifyResetTokenFull, request, cancellationToken);

    public Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync(ApiEndpoints.Auth.ResetPasswordFull, request, cancellationToken);
}
