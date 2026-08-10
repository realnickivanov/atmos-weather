namespace Atmos.Domain.Weather;

public sealed record WeatherForecast(
    WeatherLocation Location,
    CurrentWeather Current,
    IReadOnlyList<HourlyWeather> Hourly,
    IReadOnlyList<DailyWeather> Daily);
