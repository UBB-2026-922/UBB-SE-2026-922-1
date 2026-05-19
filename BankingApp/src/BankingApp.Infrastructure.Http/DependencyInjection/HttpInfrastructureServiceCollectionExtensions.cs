namespace BankingApp.Infrastructure.Http.DependencyInjection;

using Contracts.Features.AccountOverview.Services;
using Contracts.Features.Authentication.Services;
using Contracts.Features.Beneficiaries.Services;
using Contracts.Features.Billers.Services;
using Contracts.Features.BillPayments.Services;
using Contracts.Features.Cards.Services;
using Contracts.Features.Forex.Services;
using Contracts.Features.ForexRateAlerts.Services;
using Contracts.Features.RecurringPayments.Services;
using Contracts.Features.Transfers.Services;
using Contracts.Features.UserProfile.Services;
using Features.AccountOverview.Services;
using Features.Authentication.Services;
using Features.Beneficiaries.Services;
using Features.Billers.Services;
using Features.BillPayments.Services;
using Features.Cards.Services;
using Features.Forex.Services;
using Features.ForexRateAlerts.Services;
using Features.RecurringPayments.Services;
using Features.Transfers.Services;
using Features.UserProfile.Services;
using Microsoft.Extensions.DependencyInjection;

public static class HttpInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHttpInfrastructure(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(ServiceDescriptor.Describe(typeof(IAuthenticationService), typeof(AuthenticationService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IAccountOverviewService), typeof(AccountOverview), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBeneficiaryService), typeof(BeneficiaryService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBillPaymentService), typeof(BillPaymentService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IBillerService), typeof(BillerService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ICardService), typeof(CardService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IForexService), typeof(ForexService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IRateAlertService), typeof(RateAlertService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IRecurringPaymentService), typeof(RecurringPaymentService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(ITransferService), typeof(TransferService), lifetime));
        services.Add(ServiceDescriptor.Describe(typeof(IProfileService), typeof(ProfileService), lifetime));

        return services;
    }
}
