using Atmos.Application.Abstractions;
using Atmos.Infrastructure.WeatherApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atmos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<WeatherApiOptions>, WeatherApiOptionsValidator>();
        services
            .AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName))
            .ValidateOnStart();

        services.AddMemoryCache(options => options.SizeLimit = 16);

        services
            .AddHttpClient(WeatherApiClient.HttpClientName, (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<WeatherApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Atmos/1.0");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(250);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(12);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.MinimumThroughput = 5;
            });

        services.AddSingleton<WeatherApiClient>();
        services.AddSingleton<IWeatherProvider, CachedWeatherProvider>();

        return services;
    }
}
