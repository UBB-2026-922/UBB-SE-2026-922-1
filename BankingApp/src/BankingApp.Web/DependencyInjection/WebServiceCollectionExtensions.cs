namespace BankingApp.Web.DependencyInjection;

using Http;

public static class WebServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebClientServices(string apiBaseUrl)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<TokenForwardingHandler>();

            services.AddHttpClient("BankingAppApi", client =>
                    client.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<TokenForwardingHandler>();

            return services;
        }
    }
}
