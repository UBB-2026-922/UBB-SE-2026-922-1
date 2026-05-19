namespace BankingApp.Web.DependencyInjection;

using Http;
using Contracts.Features.AccountOverview.Services;
using Contracts.Features.Authentication.Services;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Billers.Services;
using Contracts.Features.Forex.Services;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Features.Transfers.Services;
using Contracts.Features.UserProfile.Services;
using BankingApp.Contracts.Http;
using BankingApp.Infrastructure.Http.Features.AccountOverview.Services;
using BankingApp.Infrastructure.Http.Features.Authentication.Services;
using BankingApp.Infrastructure.Http.Features.Beneficiaries.Services;
using BankingApp.Infrastructure.Http.Features.BillPayments.Services;
using BankingApp.Infrastructure.Http.Features.Billers.Services;
using BankingApp.Infrastructure.Http.Features.Forex.Services;
using BankingApp.Infrastructure.Http.Features.ForexRateAlerts.Services;
using BankingApp.Infrastructure.Http.Features.RecurringPayments.Services;
using BankingApp.Infrastructure.Http.Features.Transfers.Services;
using BankingApp.Infrastructure.Http.Features.UserProfile.Services;

public static class WebServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebClientServices(string apiBaseUrl)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<TokenForwardingHandler>();

            services.AddHttpClient(HttpClientNames.Api, client =>
                    client.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<TokenForwardingHandler>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<IBillPaymentService, BillPaymentService>();
            services.AddScoped<IBillerService, BillerService>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<IForexService, ForexService>();
            services.AddScoped<IRateAlertService, RateAlertService>();
            services.AddScoped<IRecurringPaymentService, RecurringPaymentService>();
            services.AddScoped<IProfileService, ProfileService>();

            return services;
        }
    }
}
