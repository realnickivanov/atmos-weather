namespace Atmos.Infrastructure.WeatherApi;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; init; } = "https://api.weatherapi.com/v1/";

    public string ApiKey { get; init; } = string.Empty;

    public string LocationQuery { get; init; } = "55.7558,37.6173";

    public int ForecastDays { get; init; } = 3;

    public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(5);
}
