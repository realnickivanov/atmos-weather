using Atmos.Application.Abstractions;
using Atmos.Application.Common.Exceptions;
using Atmos.Domain.Weather;
using MediatR;

namespace Atmos.Application.Weather.GetWeather;

public sealed class GetWeatherQueryHandler(
    IWeatherProvider weatherProvider,
    TimeProvider timeProvider)
    : IRequestHandler<GetWeatherQuery, WeatherResponse>
{
    public async Task<WeatherResponse> Handle(
        GetWeatherQuery request,
        CancellationToken cancellationToken)
    {
        var forecast = await weatherProvider.GetForecastAsync(cancellationToken);
        var localNow = GetLocalNow(forecast.Location.TimeZoneId);
        var today = DateOnly.FromDateTime(localNow.DateTime);
        var tomorrow = today.AddDays(1);

        var hourly = forecast.Hourly
            .Where(hour => IsRequiredHour(hour, localNow, today, tomorrow))
            .OrderBy(hour => hour.Time)
            .Select(MapHourly)
            .ToArray();

        var daily = forecast.Daily
            .Where(day => day.Date >= today)
            .OrderBy(day => day.Date)
            .Take(3)
            .Select(MapDaily)
            .ToArray();

        ValidateForecastCompleteness(hourly, daily, today, tomorrow);

        return new WeatherResponse(
            MapLocation(forecast.Location, localNow),
            MapCurrent(forecast.Current),
            hourly,
            daily);
    }

    private DateTimeOffset GetLocalNow(string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone);
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

    private static void ValidateForecastCompleteness(
        IReadOnlyList<HourlyWeatherResponse> hourly,
        IReadOnlyList<DailyWeatherResponse> daily,
        DateOnly today,
        DateOnly tomorrow)
    {
        var expectedDates = new[] { today, tomorrow, tomorrow.AddDays(1) };
        if (!daily.Select(day => day.Date).SequenceEqual(expectedDates))
        {
            throw new WeatherProviderUnavailableException(
                "The weather provider did not return a complete three-day forecast.");
        }

        var tomorrowHours = hourly
            .Where(hour => DateOnly.FromDateTime(hour.Time.DateTime) == tomorrow)
            .Select(hour => hour.Time.Hour)
            .Distinct()
            .Order()
            .ToArray();

        if (!tomorrowHours.SequenceEqual(Enumerable.Range(0, 24)))
        {
            throw new WeatherProviderUnavailableException(
                "The weather provider did not return every hour for tomorrow.");
        }
    }

    private static bool IsRequiredHour(
        HourlyWeather hour,
        DateTimeOffset localNow,
        DateOnly today,
        DateOnly tomorrow)
    {
        var hourDate = DateOnly.FromDateTime(hour.Time.DateTime);

        return (hourDate == today && hour.Time >= localNow)
            || hourDate == tomorrow;
    }

    private static LocationResponse MapLocation(
        WeatherLocation location,
        DateTimeOffset localNow) =>
        new(
            location.Name,
            location.Region,
            location.Country,
            location.TimeZoneId,
            location.Latitude,
            location.Longitude,
            localNow);

    private static CurrentWeatherResponse MapCurrent(CurrentWeather current) =>
        new(
            current.ObservedAt,
            current.TemperatureC,
            current.FeelsLikeC,
            current.Condition.Code,
            current.Condition.Text,
            current.Condition.IsDay,
            current.WindKph,
            current.WindDirection,
            current.GustKph,
            current.Humidity,
            current.PressureMb,
            current.PrecipitationMm,
            current.VisibilityKm,
            current.UvIndex);

    private static HourlyWeatherResponse MapHourly(HourlyWeather hourly) =>
        new(
            hourly.Time,
            hourly.TemperatureC,
            hourly.FeelsLikeC,
            hourly.Condition.Code,
            hourly.Condition.Text,
            hourly.Condition.IsDay,
            hourly.ChanceOfRain,
            hourly.Humidity,
            hourly.WindKph);

    private static DailyWeatherResponse MapDaily(DailyWeather daily) =>
        new(
            daily.Date,
            daily.MinimumTemperatureC,
            daily.MaximumTemperatureC,
            daily.AverageTemperatureC,
            daily.Condition.Code,
            daily.Condition.Text,
            daily.ChanceOfRain,
            daily.MaximumWindKph,
            daily.AverageHumidity,
            daily.UvIndex,
            daily.Sunrise,
            daily.Sunset);
}
