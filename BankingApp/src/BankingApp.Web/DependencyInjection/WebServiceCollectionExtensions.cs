namespace BankingApp.Web.DependencyInjection;

using Http;
using BankingApp.Contracts.Http;
using BankingApp.Infrastructure.Http.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddHttpInfrastructure(ServiceLifetime.Scoped);

            return services;
        }
    }
}
