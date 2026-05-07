using System.Threading.Tasks;
using BankingApp.Application.DTOs.Auth;
using ErrorOr;

namespace BankingApp.Desktop.Services;

public interface IAuthClientService
{
    int? CurrentUserId { get; set; }

    ErrorOr<Success> EnsureConfigured();

    Task<ErrorOr<LoginSuccessResponse>> LoginAsync(string email, string password);

    void SetToken(string token);

    Task<ErrorOr<Success>> RegisterAsync(string email, string password, string fullName);

    Task<ErrorOr<LoginSuccessResponse>> VerifyOtpAsync(int userId, string otpCode);

    Task<ErrorOr<object>> ResendOtpAsync(int userId);
}
