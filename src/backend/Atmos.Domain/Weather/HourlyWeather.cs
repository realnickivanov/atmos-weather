namespace Atmos.Domain.Weather;

public sealed record HourlyWeather(
    DateTimeOffset Time,
    decimal TemperatureC,
    decimal FeelsLikeC,
    WeatherCondition Condition,
    int ChanceOfRain,
    int Humidity,
    decimal WindKph);
