using System.Globalization;
using Atmos.Application.Common.Exceptions;
using Atmos.Domain.Weather;
using Atmos.Infrastructure.WeatherApi.Models;

namespace Atmos.Infrastructure.WeatherApi;

internal static class WeatherApiMapper
{
    private const int RequiredForecastDayCount = 3;
    private const int RequiredHoursPerDay = 24;

    public static WeatherForecast Map(WeatherApiForecastResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var location = response.Location
            ?? throw Malformed("The provider response did not contain a location.");
        var current = response.Current
            ?? throw Malformed("The provider response did not contain current weather.");
        var providerForecast = response.Forecast
            ?? throw Malformed("The provider response did not contain a forecast.");
        var providerDays = providerForecast.ForecastDays
            ?? throw Malformed("The provider response did not contain forecast days.");

        if (providerDays.Count < RequiredForecastDayCount)
        {
            throw Malformed("The provider response contained fewer than three forecast days.");
        }

        ValidateCoordinates(location.Latitude, location.Longitude);
        var locationName = RequiredText(location.Name, "location name");
        var country = RequiredText(location.Country, "country");
        var timeZoneId = RequiredText(location.TimeZoneId, "time zone");
        var timeZone = ResolveTimeZone(timeZoneId);
        var localTime = ConvertEpoch(location.LocalTimeEpoch, timeZone, "location local time");

        var mappedLocation = new WeatherLocation(
            locationName,
            location.Region?.Trim() ?? string.Empty,
            country,
            timeZoneId,
            location.Latitude,
            location.Longitude,
            localTime);

        var mappedCurrent = MapCurrent(current, timeZone);
        var mappedDays = providerDays
            .Take(RequiredForecastDayCount)
            .Select(day => MapForecastDay(day, timeZone))
            .OrderBy(day => day.Daily.Date)
            .ToArray();

        ValidateConsecutiveDates(mappedDays, localTime);

        return new WeatherForecast(
            mappedLocation,
            mappedCurrent,
            mappedDays.SelectMany(day => day.Hourly).ToArray(),
            mappedDays.Select(day => day.Daily).ToArray());
    }

    private static CurrentWeather MapCurrent(WeatherApiCurrent current, TimeZoneInfo timeZone) =>
        new(
            ConvertEpoch(current.LastUpdatedEpoch, timeZone, "current observation time"),
            current.TemperatureC,
            current.FeelsLikeC,
            MapCondition(current.Condition, MapIsDay(current.IsDay, "current weather")),
            NonNegative(current.WindKph, "current wind speed"),
            RequiredText(current.WindDirection, "wind direction"),
            NonNegative(current.GustKph, "current wind gust"),
            Percentage(current.Humidity, "current humidity"),
            Positive(current.PressureMb, "current pressure"),
            NonNegative(current.PrecipitationMm, "current precipitation"),
            NonNegative(current.VisibilityKm, "current visibility"),
            NonNegative(current.UvIndex, "current UV index"));

    private static MappedForecastDay MapForecastDay(
        WeatherApiForecastDay? forecastDay,
        TimeZoneInfo timeZone)
    {
        if (forecastDay is null)
        {
            throw Malformed("The provider response contained an empty forecast day.");
        }

        var date = ParseDate(forecastDay.Date);
        var day = forecastDay.Day
            ?? throw Malformed("A forecast day did not contain summary data.");
        var astro = forecastDay.Astro
            ?? throw Malformed("A forecast day did not contain astronomy data.");
        var providerHours = forecastDay.Hours
            ?? throw Malformed("A forecast day did not contain hourly data.");

        if (providerHours.Count != RequiredHoursPerDay)
        {
            throw Malformed("A forecast day did not contain all 24 hourly entries.");
        }

        if (day.MinimumTemperatureC > day.MaximumTemperatureC
            || day.AverageTemperatureC < day.MinimumTemperatureC
            || day.AverageTemperatureC > day.MaximumTemperatureC)
        {
            throw Malformed("A forecast day contained an invalid temperature range.");
        }

        var hourly = providerHours
            .Select(hour => MapHourly(hour, timeZone, date))
            .OrderBy(hour => hour.Time)
            .ToArray();

        if (hourly.DistinctBy(hour => hour.Time).Count() != RequiredHoursPerDay)
        {
            throw Malformed("A forecast day contained duplicate hourly entries.");
        }

        var daily = new DailyWeather(
            date,
            day.MinimumTemperatureC,
            day.MaximumTemperatureC,
            day.AverageTemperatureC,
            MapCondition(day.Condition, isDay: true),
            Percentage(day.DailyChanceOfRain, "daily chance of rain"),
            NonNegative(day.MaximumWindKph, "daily maximum wind speed"),
            Percentage(day.AverageHumidity, "daily average humidity"),
            NonNegative(day.UvIndex, "daily UV index"),
            RequiredText(astro.Sunrise, "sunrise"),
            RequiredText(astro.Sunset, "sunset"));

        return new MappedForecastDay(daily, hourly);
    }

    private static HourlyWeather MapHourly(
        WeatherApiHour? hour,
        TimeZoneInfo timeZone,
        DateOnly expectedDate)
    {
        if (hour is null)
        {
            throw Malformed("A forecast day contained an empty hourly entry.");
        }

        var time = ConvertEpoch(hour.TimeEpoch, timeZone, "hourly forecast time");
        if (DateOnly.FromDateTime(time.DateTime) != expectedDate)
        {
            throw Malformed("An hourly entry did not belong to its forecast day.");
        }

        return new HourlyWeather(
            time,
            hour.TemperatureC,
            hour.FeelsLikeC,
            MapCondition(hour.Condition, MapIsDay(hour.IsDay, "hourly weather")),
            Percentage(hour.ChanceOfRain, "hourly chance of rain"),
            Percentage(hour.Humidity, "hourly humidity"),
            NonNegative(hour.WindKph, "hourly wind speed"));
    }

    private static WeatherCondition MapCondition(
        WeatherApiCondition? condition,
        bool isDay)
    {
        if (condition is null || condition.Code <= 0)
        {
            throw Malformed("The provider response contained an invalid weather condition.");
        }

        return new WeatherCondition(
            condition.Code,
            RequiredText(condition.Text, "weather condition"),
            isDay);
    }

    private static void ValidateConsecutiveDates(
        IReadOnlyList<MappedForecastDay> forecastDays,
        DateTimeOffset localTime)
    {
        var expectedDate = DateOnly.FromDateTime(localTime.DateTime);

        for (var index = 0; index < RequiredForecastDayCount; index++)
        {
            if (forecastDays[index].Daily.Date != expectedDate.AddDays(index))
            {
                throw Malformed("Forecast dates were not consecutive from the location's local date.");
            }
        }
    }

    private static DateOnly ParseDate(string? value)
    {
        if (!DateOnly.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            throw Malformed("A forecast day contained an invalid date.");
        }

        return date;
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new WeatherProviderUnavailableException(
                "The weather provider returned an unsupported time zone.",
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new WeatherProviderUnavailableException(
                "The weather provider returned an invalid time zone.",
                exception);
        }
    }

    private static DateTimeOffset ConvertEpoch(
        long epoch,
        TimeZoneInfo timeZone,
        string fieldName)
    {
        if (epoch <= 0)
        {
            throw Malformed($"The provider response contained an invalid {fieldName}.");
        }

        try
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(epoch), timeZone);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new WeatherProviderUnavailableException(
                $"The provider response contained an invalid {fieldName}.",
                exception);
        }
    }

    private static string RequiredText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Malformed($"The provider response did not contain a valid {fieldName}.");
        }

        return value.Trim();
    }

    private static bool MapIsDay(int value, string fieldName) => value switch
    {
        0 => false,
        1 => true,
        _ => throw Malformed($"The provider response contained an invalid day/night flag for {fieldName}."),
    };

    private static int Percentage(int value, string fieldName)
    {
        if (value is < 0 or > 100)
        {
            throw Malformed($"The provider response contained an invalid {fieldName}.");
        }

        return value;
    }

    private static decimal NonNegative(decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw Malformed($"The provider response contained an invalid {fieldName}.");
        }

        return value;
    }

    private static decimal Positive(decimal value, string fieldName)
    {
        if (value <= 0)
        {
            throw Malformed($"The provider response contained an invalid {fieldName}.");
        }

        return value;
    }

    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            throw Malformed("The provider response contained invalid coordinates.");
        }
    }

    private static WeatherProviderUnavailableException Malformed(string message) =>
        new(message);

    private sealed record MappedForecastDay(
        DailyWeather Daily,
        IReadOnlyList<HourlyWeather> Hourly);
}
