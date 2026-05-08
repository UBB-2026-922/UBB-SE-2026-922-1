namespace BankingApp.Application.Common.Contracts.Notifications;

public interface IEmailService
{
    public Task SendOtpCodeAsync(string email, string code);
    public Task SendLoginAlertAsync(string email);
    public Task SendPasswordResetLinkAsync(string email, string rawToken);
}
