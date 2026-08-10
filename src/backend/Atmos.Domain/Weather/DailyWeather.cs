namespace Atmos.Domain.Weather;

public sealed record DailyWeather(
    DateOnly Date,
    decimal MinimumTemperatureC,
    decimal MaximumTemperatureC,
    decimal AverageTemperatureC,
    WeatherCondition Condition,
    int ChanceOfRain,
    decimal MaximumWindKph,
    int AverageHumidity,
    decimal UvIndex,
    string Sunrise,
    string Sunset);
