using Atmos.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Atmos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
        });

        return services;
    }
}
