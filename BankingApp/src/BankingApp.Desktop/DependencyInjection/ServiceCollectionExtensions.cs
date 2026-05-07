namespace BankingApp.Desktop.DependencyInjection;

using Master;
using BankingApp.Desktop.Services;
using Services.Transfers;
using ViewModels;
using Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// TODO: add docs.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IRegistrationContext, RegistrationContext>();

        services.AddTransient<IAuthClientService, AuthClientService>();
        services.AddTransient<IDashboardClientService, DashboardClientService>();
        services.AddTransient<IProfileClientService, ProfileClientService>();
        services.AddTransient<IForexClientService, ForexClientService>();
        services.AddTransient<IRateAlertClientService, RateAlertClientService>();
        services.AddTransient<IBillPaymentClientService, BillPaymentClientService>();
        services.AddTransient<ITransferClientService, TransferClientService>();

        services.AddTransient<IPasswordRecoveryManager>(provider =>
        {
            IApiClient apiClient = provider.GetRequiredService<IApiClient>();
            return new PasswordRecoveryManager(apiClient, new SystemClock());
        });

        services.AddTransient<ICountdownTimer, DispatcherCountdownTimer>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<TwoFactorViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PersonalInfoViewModel>();
        services.AddTransient<SecurityViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<BeneficiariesViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ForexViewModel>();
        services.AddTransient<RateAlertViewModel>();
        services.AddTransient<TransferViewModel>();
        services.AddTransient<TransferHistoryViewModel>();
        services.AddTransient<BillPayViewModel>();
        services.AddTransient<RecurringPaymentViewModel>();

        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<TwoFactorView>();
        services.AddTransient<ForgotPasswordView>();
        services.AddTransient<NavView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<BeneficiariesView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<ForexPage>();
        services.AddTransient<RateAlertsPage>();
        services.AddTransient<BillPayView>();
        services.AddTransient<RecurringPaymentView>();
        services.AddTransient<TransferView>();
        services.AddTransient<TransferHistoryView>();
        return services;
    }
}
