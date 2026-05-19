namespace BankingApp.Application.Features.Authentication.Services;

using Contracts.Features.Authentication.Dtos;
using Contracts.Features.PasswordReset.Dtos;
using Contracts.Features.UserRegistration.Dtos;
using ErrorOr;

/// <summary>Defines authentication-related API operations used by client applications.</summary>
public interface IAuthenticationService
{
    public Task<ErrorOr<LoginSuccessResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    public Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> ResendOtpAsync(int userId, CancellationToken ct = default);
    public Task<ErrorOr<Success>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> VerifyResetTokenAsync(VerifyResetTokenRequest request, CancellationToken ct = default);
    public Task<ErrorOr<Success>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
