using System.Text.Json.Serialization;

namespace Atmos.Infrastructure.WeatherApi.Models;

internal sealed class WeatherApiForecastResponse
{
    [JsonPropertyName("location")]
    public WeatherApiLocation? Location { get; init; }

    [JsonPropertyName("current")]
    public WeatherApiCurrent? Current { get; init; }

    [JsonPropertyName("forecast")]
    public WeatherApiForecast? Forecast { get; init; }
}

internal sealed class WeatherApiLocation
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; init; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; init; } = string.Empty;

    [JsonPropertyName("lat")]
    public decimal Latitude { get; init; }

    [JsonPropertyName("lon")]
    public decimal Longitude { get; init; }

    [JsonPropertyName("tz_id")]
    public string TimeZoneId { get; init; } = string.Empty;

    [JsonPropertyName("localtime_epoch")]
    public long LocalTimeEpoch { get; init; }
}

internal sealed class WeatherApiCurrent
{
    [JsonPropertyName("last_updated_epoch")]
    public long LastUpdatedEpoch { get; init; }

    [JsonPropertyName("temp_c")]
    public decimal TemperatureC { get; init; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; init; }

    [JsonPropertyName("condition")]
    public WeatherApiCondition? Condition { get; init; }

    [JsonPropertyName("wind_kph")]
    public decimal WindKph { get; init; }

    [JsonPropertyName("wind_dir")]
    public string WindDirection { get; init; } = string.Empty;

    [JsonPropertyName("pressure_mb")]
    public decimal PressureMb { get; init; }

    [JsonPropertyName("precip_mm")]
    public decimal PrecipitationMm { get; init; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; init; }

    [JsonPropertyName("feelslike_c")]
    public decimal FeelsLikeC { get; init; }

    [JsonPropertyName("vis_km")]
    public decimal VisibilityKm { get; init; }

    [JsonPropertyName("uv")]
    public decimal UvIndex { get; init; }

    [JsonPropertyName("gust_kph")]
    public decimal GustKph { get; init; }
}

internal sealed class WeatherApiForecast
{
    [JsonPropertyName("forecastday")]
    public IReadOnlyList<WeatherApiForecastDay?>? ForecastDays { get; init; }
}

internal sealed class WeatherApiForecastDay
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("day")]
    public WeatherApiDay? Day { get; init; }

    [JsonPropertyName("astro")]
    public WeatherApiAstro? Astro { get; init; }

    [JsonPropertyName("hour")]
    public IReadOnlyList<WeatherApiHour?>? Hours { get; init; }
}

internal sealed class WeatherApiDay
{
    [JsonPropertyName("maxtemp_c")]
    public decimal MaximumTemperatureC { get; init; }

    [JsonPropertyName("mintemp_c")]
    public decimal MinimumTemperatureC { get; init; }

    [JsonPropertyName("avgtemp_c")]
    public decimal AverageTemperatureC { get; init; }

    [JsonPropertyName("maxwind_kph")]
    public decimal MaximumWindKph { get; init; }

    [JsonPropertyName("avghumidity")]
    public int AverageHumidity { get; init; }

    [JsonPropertyName("daily_chance_of_rain")]
    public int DailyChanceOfRain { get; init; }

    [JsonPropertyName("condition")]
    public WeatherApiCondition? Condition { get; init; }

    [JsonPropertyName("uv")]
    public decimal UvIndex { get; init; }
}

internal sealed class WeatherApiAstro
{
    [JsonPropertyName("sunrise")]
    public string Sunrise { get; init; } = string.Empty;

    [JsonPropertyName("sunset")]
    public string Sunset { get; init; } = string.Empty;
}

internal sealed class WeatherApiHour
{
    [JsonPropertyName("time_epoch")]
    public long TimeEpoch { get; init; }

    [JsonPropertyName("temp_c")]
    public decimal TemperatureC { get; init; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; init; }

    [JsonPropertyName("condition")]
    public WeatherApiCondition? Condition { get; init; }

    [JsonPropertyName("wind_kph")]
    public decimal WindKph { get; init; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; init; }

    [JsonPropertyName("feelslike_c")]
    public decimal FeelsLikeC { get; init; }

    [JsonPropertyName("chance_of_rain")]
    public int ChanceOfRain { get; init; }
}

internal sealed class WeatherApiCondition
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; init; }
}
