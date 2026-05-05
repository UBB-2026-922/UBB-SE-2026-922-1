namespace BankingApp.Desktop.DependencyInjection;

using Master;
using Repositories;
using Utilities;
using ViewModels;
using Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register desktop-client services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all necessary client-side services, repositories, and view models to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The modified service collection.</returns>
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

        // Repositories abstract the HTTP transport from ViewModels.
        services.AddTransient<IForexRepository, ForexRepository>();
        services.AddTransient<IRateAlertRepository, RateAlertRepository>();

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

        // Views are registered as transient so the navigation service can resolve them
        // through the container. Each navigation gets a fresh page instance with all
        // constructor dependencies (ViewModels, NavigationService) injected automatically.
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
