namespace Atmos.Domain.Weather;

public sealed record CurrentWeather(
    DateTimeOffset ObservedAt,
    decimal TemperatureC,
    decimal FeelsLikeC,
    WeatherCondition Condition,
    decimal WindKph,
    string WindDirection,
    decimal GustKph,
    int Humidity,
    decimal PressureMb,
    decimal PrecipitationMm,
    decimal VisibilityKm,
    decimal UvIndex);
