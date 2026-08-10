namespace Atmos.Domain.Weather;

public sealed record WeatherLocation(
    string Name,
    string Region,
    string Country,
    string TimeZoneId,
    decimal Latitude,
    decimal Longitude,
    DateTimeOffset LocalTime);
