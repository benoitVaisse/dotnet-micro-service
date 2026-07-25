using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Messaging.DomaineEvents;
using Ordering.Infrastructure.Interceptors;
using Ordering.Infrastructure.Messaging;

namespace Ordering.Infrastructure;

public static class ConfigurationContext
{
    public static IServiceCollection AddOrderingContext(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddInterceptor();

        return services;
    }

    public static IServiceCollection AddInterceptor(this IServiceCollection services)
    {
        services.AddScoped<DomaineEventInterceptor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
