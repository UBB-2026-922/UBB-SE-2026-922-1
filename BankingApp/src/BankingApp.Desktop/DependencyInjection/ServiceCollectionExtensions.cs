namespace BankingApp.Desktop.DependencyInjection;

using System;
using Application.Common.Http;
using Infrastructure.Http.Common.Http;
using Contracts.Features.AccountOverview.Services;
using Contracts.Features.Authentication.Services;
using Contracts.Features.Billers.Services;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Cards.Services;
using Contracts.Features.Forex.Services;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Features.Transfers.Services;
using Contracts.Features.UserProfile.Services;
using ViewModels;
using Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Http;
using Infrastructure.Http.Features.AccountOverview.Services;
using Infrastructure.Http.Features.Authentication.Services;
using Infrastructure.Http.Features.Beneficiaries.Services;
using Infrastructure.Http.Features.Billers.Services;
using Infrastructure.Http.Features.BillPayments.Services;
using Infrastructure.Http.Features.Cards.Services;
using Infrastructure.Http.Features.Forex.Services;
using Infrastructure.Http.Features.ForexRateAlerts.Services;
using Infrastructure.Http.Features.RecurringPayments.Services;
using Infrastructure.Http.Features.Transfers.Services;
using Infrastructure.Http.Features.UserProfile.Services;
using Navigation;
using Utilities;

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
        services.AddHttpClient(HttpClientNames.Api, client =>
        {
            string? baseUrl = configuration["ApiBaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl);
            }
        });

        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IRegistrationContext, RegistrationContext>();

        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IPasswordRecoveryManager>(provider =>
            new PasswordRecoveryManager(
                provider.GetRequiredService<IApiClient>(),
                new SystemClock()));

        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.AddTransient<IBeneficiaryService, BeneficiaryService>();
        services.AddTransient<IBillPaymentService, BillPaymentService>();
        services.AddTransient<IBillerService, BillerService>();
        services.AddTransient<ICardService, CardService>();
        services.AddTransient<IForexService, ForexService>();
        services.AddTransient<IRateAlertService, RateAlertService>();
        services.AddTransient<IRecurringPaymentService, RecurringPaymentService>();
        services.AddTransient<ITransferService, TransferService>();
        services.AddTransient<IProfileService, ProfileService>();

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
