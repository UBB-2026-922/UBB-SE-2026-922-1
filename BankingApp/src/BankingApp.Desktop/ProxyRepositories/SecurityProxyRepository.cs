namespace BankingApp.Desktop.ProxyRepositories;

using System.Threading.Tasks;
using BankingApp.Desktop.Utilities;
using ErrorOr;

internal sealed class SecurityProxyRepository
{
    private readonly IApiClient _apiClient;

    public SecurityProxyRepository(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ErrorOr<string>> HashAsync(string input)
        => _apiClient.PostAsync<HashRequest, string>("/api/raw/security/hash", new HashRequest { Input = input });

    public Task<ErrorOr<bool>> VerifyHashAsync(string input, string hash)
        => _apiClient.PostAsync<VerifyHashRequest, bool>(
            "/api/raw/security/verify-hash",
            new VerifyHashRequest { Input = input, Hash = hash });

    public Task<ErrorOr<string>> GenerateTokenAsync(int userId)
        => _apiClient.PostAsync<GenerateTokenRequest, string>(
            "/api/raw/security/jwt/generate",
            new GenerateTokenRequest { UserId = userId });

    public Task<ErrorOr<string>> GenerateTotpAsync(int userId)
        => _apiClient.PostAsync<OtpUserRequest, string>(
            "/api/raw/security/otp/generate-totp",
            new OtpUserRequest { UserId = userId });

    public Task<ErrorOr<bool>> VerifyTotpAsync(int userId, string code)
        => _apiClient.PostAsync<VerifyOtpRequest, bool>(
            "/api/raw/security/otp/verify-totp",
            new VerifyOtpRequest { UserId = userId, Code = code });

    public Task<ErrorOr<string>> GenerateSmsOtpAsync(int userId)
        => _apiClient.PostAsync<OtpUserRequest, string>(
            "/api/raw/security/otp/generate-sms",
            new OtpUserRequest { UserId = userId });

    public Task<ErrorOr<bool>> VerifySmsOtpAsync(int userId, string code)
        => _apiClient.PostAsync<VerifyOtpRequest, bool>(
            "/api/raw/security/otp/verify-sms",
            new VerifyOtpRequest { UserId = userId, Code = code });

    public Task<ErrorOr<Success>> InvalidateOtpAsync(int userId)
        => _apiClient.PostAsync("/api/raw/security/otp/invalidate", new OtpUserRequest { UserId = userId });

    public Task<ErrorOr<Success>> SendPasswordResetLinkAsync(string email, string token)
        => _apiClient.PostAsync(
            "/api/raw/security/email/send-password-reset-link",
            new PasswordResetEmailRequest { Email = email, Token = token });

    public Task<ErrorOr<Success>> SendOtpCodeAsync(string email, string code)
        => _apiClient.PostAsync("/api/raw/security/email/send-otp", new OtpEmailRequest { Email = email, Code = code });

    public Task<ErrorOr<Success>> SendLoginAlertAsync(string email)
        => _apiClient.PostAsync("/api/raw/security/email/send-login-alert", new EmailRequest { Email = email });

    public Task<ErrorOr<Success>> SendLockNotificationAsync(string email)
        => _apiClient.PostAsync("/api/raw/security/email/send-lock-notification", new EmailRequest { Email = email });

    private sealed class HashRequest
    {
        public string Input { get; set; } = string.Empty;
    }

    private sealed class VerifyHashRequest
    {
        public string Input { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;
    }

    private sealed class GenerateTokenRequest
    {
        public int UserId { get; set; }
    }

    private sealed class OtpUserRequest
    {
        public int UserId { get; set; }
    }

    private sealed class VerifyOtpRequest
    {
        public int UserId { get; set; }

        public string Code { get; set; } = string.Empty;
    }

    private sealed class EmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    private sealed class OtpEmailRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }

    private sealed class PasswordResetEmailRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}
