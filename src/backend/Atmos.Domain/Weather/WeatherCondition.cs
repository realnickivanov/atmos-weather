namespace Atmos.Domain.Weather;

public sealed record WeatherCondition(
    int Code,
    string Text,
    bool IsDay);
