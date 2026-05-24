namespace BankingApp.Application.Common.Notifications;

public interface IEmailService
{
    public Task SendLoginAlertAsync(string email);
    public Task SendPasswordResetLinkAsync(string email, string rawToken);
}
