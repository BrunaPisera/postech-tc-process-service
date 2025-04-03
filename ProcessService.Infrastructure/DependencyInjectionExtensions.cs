using Microsoft.Extensions.DependencyInjection;
using ProcessService.Infrastructure.Broker;

namespace ProcessService.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IBrokerConnection, BrokerConnection>();
            services.AddScoped<BrokerConsumer>();

            return services;
        }
    }
}