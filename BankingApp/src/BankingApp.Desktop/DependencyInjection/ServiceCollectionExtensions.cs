namespace BankingApp.Desktop.DependencyInjection;

using System;
using Application.Shared.Http;
using ViewModels;
using Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Http;
using Features.Authentication;
using Features.PasswordRecovery;
using Features.Registration;
using Infrastructure.Core.DependencyInjection;
using Infrastructure.Http.DependencyInjection;
using Infrastructure.Http.Shared.Http;
using Navigation;
using Shared.Timers;

/// <summary>Registers the desktop application's client services, view models, and views.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the Desktop layer dependencies required by the WinUI client.</summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">Application configuration used by HTTP and client services.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddTransient<AuthenticationSessionTokenHandler>();
        services.AddHttpClient(HttpClientNames.Api, client =>
            {
                string? baseUrl = configuration["ApiBaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            })
            .AddHttpMessageHandler<AuthenticationSessionTokenHandler>();

        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IRegistrationContext, RegistrationContext>();
        services.AddSingleton<IAuthenticationSession, AuthenticationSession>();
        services.AddCoreInfrastructure(configuration);

        services.AddTransient<IPasswordRecoveryService, PasswordRecoveryService>();

        services.AddHttpInfrastructure();

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

        services.AddTransient<CardViewModel>();

        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<TwoFactorView>();
        services.AddTransient<ForgotPasswordView>();
        services.AddTransient<NavigationView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<BeneficiariesView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<ForexPage>();
        services.AddTransient<RateAlertsPage>();
        services.AddTransient<BillPayView>();
        services.AddTransient<RecurringPaymentView>();
        services.AddTransient<TransferView>();
        services.AddTransient<TransferHistoryView>();
        services.AddTransient<CardsView>();
        return services;
    }
}
