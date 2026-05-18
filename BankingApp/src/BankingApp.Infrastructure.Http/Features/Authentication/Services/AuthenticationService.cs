namespace BankingApp.Infrastructure.Http.Features.Authentication.Services;

using Contracts.Features.Authentication.Dtos;
using Contracts.Features.Authentication.Services;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using ErrorOr;

public sealed class AuthenticationService(IHttpClientFactory httpClientFactory) : IAuthenticationService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);

    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Login, request, ct);

    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.Register, request, ct);

    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.VerifyOtp, request, ct);

    public Task<ErrorOr<Success>> ResendOtpAsync(int userId, CancellationToken ct = default)
        => _http.PostErrorOrAsync($"{ApiEndpoints.ResendOtp}?userId={userId}", new { }, ct);

    public Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.ForgotPassword, request, ct);

    public Task<ErrorOr<Success>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.VerifyResetToken, request, ct);

    public Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.ResetPassword, request, ct);
}
