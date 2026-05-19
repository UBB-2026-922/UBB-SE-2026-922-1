namespace BankingApp.Application.Common.Security;

using ErrorOr;

public interface IOtpService
{
    public ErrorOr<string> GenerateTotp(int userId);
    public ErrorOr<string> GenerateSmsOtp(int userId);

    // NOTE: Should this return ErrorOr<bool>/ErrorOr<Success>/bool?
    public ErrorOr<bool> VerifyTotp(int userId, string token);
    public ErrorOr<bool> VerifySmsOtp(int userId, string token);
    public void InvalidateOtp(int userId);
}
