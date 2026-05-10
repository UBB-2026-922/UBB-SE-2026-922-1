namespace BankingApp.Desktop.DependencyInjection;

using Application.Repositories.Interfaces;
using Application.Services.Login;
using Master;
using ProxyRepositories;
using Services;
using Utilities;
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
        services.AddSingleton<ApiClient>();
        services.AddSingleton<IApiClient>(sp => sp.GetRequiredService<ApiClient>());
        services.AddSingleton<ICurrentSession>(sp => sp.GetRequiredService<ApiClient>());
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IRegistrationContext, RegistrationContext>();
        services.AddSingleton<IOtpAttemptTracker, DesktopOtpAttemptTracker>();

        services.AddTransient<IAuthClientService, AuthClientService>();
        services.AddTransient<IAuthRepository, AuthProxyRepository>();
        services.AddTransient<SecurityProxyRepository>();
        services.AddTransient<IUserRepository, UserProxyRepository>();
        services.AddTransient<IDashboardClientService, DashboardClientService>();
        services.AddTransient<IProfileClientService, ProfileClientService>();
        services.AddTransient<IExchangeRepository, ExchangeProxyRepository>();
        services.AddTransient<IForexClientService, ForexClientService>();
        services.AddTransient<IRateAlertRepository, RateAlertProxyRepository>();
        services.AddTransient<IRateAlertClientService, RateAlertClientService>();
        services.AddTransient<IBillerRepository, BillerProxyRepository>();
        services.AddTransient<IBillPaymentRepository, BillPaymentProxyRepository>();
        services.AddTransient<IRecurringPaymentRepository, RecurringPaymentProxyRepository>();
        services.AddTransient<IBillPaymentClientService, BillPaymentClientService>();
        services.AddTransient<IDashboardRepository, DashboardProxyRepository>();
        services.AddTransient<IBeneficiaryRepository, BeneficiaryProxyRepository>();
        services.AddTransient<ITransferService, TransferService>();

        services.AddTransient<IPasswordRecoveryManager>(provider =>
        {
            IAuthClientService authClientService = provider.GetRequiredService<IAuthClientService>();
            return new PasswordRecoveryManager(authClientService, new SystemClock());
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
