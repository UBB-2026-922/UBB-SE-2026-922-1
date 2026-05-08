namespace BankingApp.Application.Common.Contracts.Notifications;

public interface IEmailService
{
    public void SendOtpCode(string email, string code);
    public void SendLoginAlert(string email);
    public void SendPasswordResetLink(string email, string rawToken);
}
