namespace BankingApp.Infrastructure.Http.Features.Authentication.Services;

using Contracts.Features.Authentication.Dtos;
using Contracts.Features.Authentication.Services;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using Contracts.Http;
using ErrorOr;
using Microsoft.Extensions.Logging;

public sealed class AuthenticationService(IHttpClientFactory httpClientFactory, ILogger<AuthenticationService> logger) : IAuthenticationService
{
    private readonly HttpClient _http = httpClientFactory.CreateClient(HttpClientNames.Api);
    private readonly ILogger<AuthenticationService> _logger = logger;

    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<LoginRequest, LoginSuccessResponse>(ApiEndpoints.Login, request, _logger, ct);

    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.Register, request, _logger, ct);

    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync<VerifyOtpRequest, LoginSuccessResponse>(ApiEndpoints.VerifyOtp, request, _logger, ct);

    public Task<ErrorOr<Success>> ResendOtpAsync(int userId, CancellationToken ct = default)
        => _http.PostErrorOrAsync($"{ApiEndpoints.ResendOtp}?userId={userId}", new { }, _logger, ct);

    public Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.ForgotPassword, request, _logger, ct);

    public Task<ErrorOr<Success>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.VerifyResetToken, request, _logger, ct);

    public Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => _http.PostErrorOrAsync(ApiEndpoints.ResetPassword, request, _logger, ct);
}
