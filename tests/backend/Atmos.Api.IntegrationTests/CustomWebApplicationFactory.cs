using Atmos.Application.Abstractions;
using Atmos.Domain.Weather;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atmos.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory(IWeatherProvider weatherProvider)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(FindApiContentRoot());
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WeatherApi:ApiKey"] = "integration-test-key",
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWeatherProvider>();
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(weatherProvider);
            services.AddSingleton<TimeProvider>(
                new FixedTimeProvider(
                    new DateTimeOffset(2026, 8, 10, 13, 15, 0, TimeSpan.Zero)));
        });
    }

    public static IWeatherProvider CreateSuccessfulProvider()
    {
        var offset = TimeSpan.FromHours(3);
        var localTime = new DateTimeOffset(2026, 8, 10, 16, 15, 0, offset);
        var condition = new WeatherCondition(1003, "Partly cloudy", IsDay: true);
        var hourlyStart = new DateTimeOffset(2026, 8, 10, 0, 0, 0, offset);
        var hourly = Enumerable.Range(0, 48)
            .Select(index => new HourlyWeather(
                hourlyStart.AddHours(index),
                20 + index % 7,
                21 + index % 7,
                condition,
                10,
                60,
                12))
            .ToArray();
        var daily = Enumerable.Range(0, 3)
            .Select(index => new DailyWeather(
                DateOnly.FromDateTime(localTime.DateTime).AddDays(index),
                15,
                25,
                20,
                condition,
                20,
                20,
                62,
                4,
                "05:10 AM",
                "08:25 PM"))
            .ToArray();
        var forecast = new WeatherForecast(
            new WeatherLocation(
                "Moscow",
                "Moscow City",
                "Russia",
                "Europe/Moscow",
                55.75m,
                37.62m,
                localTime),
            new CurrentWeather(
                localTime,
                24,
                25,
                condition,
                12,
                "WSW",
                18,
                58,
                1013,
                0,
                10,
                4),
            hourly,
            daily);

        return new StubWeatherProvider(_ => Task.FromResult(forecast));
    }

    public static IWeatherProvider CreateFailingProvider(Exception exception) =>
        new StubWeatherProvider(_ => Task.FromException<WeatherForecast>(exception));

    private static string FindApiContentRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "backend",
                "Atmos.Api");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Atmos.Api content root.");
    }

    private sealed class StubWeatherProvider(
        Func<CancellationToken, Task<WeatherForecast>> implementation)
        : IWeatherProvider
    {
        public Task<WeatherForecast> GetForecastAsync(CancellationToken cancellationToken) =>
            implementation(cancellationToken);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
