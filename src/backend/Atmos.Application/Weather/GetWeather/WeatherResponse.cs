namespace Atmos.Application.Weather.GetWeather;

public sealed record WeatherResponse(
    LocationResponse Location,
    CurrentWeatherResponse Current,
    IReadOnlyList<HourlyWeatherResponse> Hourly,
    IReadOnlyList<DailyWeatherResponse> Daily);

public sealed record LocationResponse(
    string Name,
    string Region,
    string Country,
    string TimeZoneId,
    decimal Latitude,
    decimal Longitude,
    DateTimeOffset LocalTime);

public sealed record CurrentWeatherResponse(
    DateTimeOffset ObservedAt,
    decimal TemperatureC,
    decimal FeelsLikeC,
    int ConditionCode,
    string ConditionText,
    bool IsDay,
    decimal WindKph,
    string WindDirection,
    decimal GustKph,
    int Humidity,
    decimal PressureMb,
    decimal PrecipitationMm,
    decimal VisibilityKm,
    decimal UvIndex);

public sealed record HourlyWeatherResponse(
    DateTimeOffset Time,
    decimal TemperatureC,
    decimal FeelsLikeC,
    int ConditionCode,
    string ConditionText,
    bool IsDay,
    int ChanceOfRain,
    int Humidity,
    decimal WindKph);

public sealed record DailyWeatherResponse(
    DateOnly Date,
    decimal MinimumTemperatureC,
    decimal MaximumTemperatureC,
    decimal AverageTemperatureC,
    int ConditionCode,
    string ConditionText,
    int ChanceOfRain,
    decimal MaximumWindKph,
    int AverageHumidity,
    decimal UvIndex,
    string Sunrise,
    string Sunset);
