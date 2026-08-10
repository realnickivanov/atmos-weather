using Atmos.Application.Abstractions;
using Atmos.Application.Common.Exceptions;
using Atmos.Application.Weather.GetWeather;
using Atmos.Domain.Weather;
using NSubstitute;

namespace Atmos.Application.UnitTests;

public sealed class GetWeatherQueryHandlerTests
{
    [Theory]
    [InlineData(0, 0, 48)]
    [InlineData(10, 0, 38)]
    [InlineData(10, 15, 37)]
    [InlineData(22, 59, 25)]
    [InlineData(23, 30, 24)]
    public async Task HandleReturnsRemainingHoursTodayAndAllHoursTomorrow(
        int hour,
        int minute,
        int expectedCount)
    {
        var provider = Substitute.For<IWeatherProvider>();
        var forecast = CreateForecast(hour, minute);
        provider.GetForecastAsync(Arg.Any<CancellationToken>()).Returns(forecast);
        var handler = CreateHandler(provider, forecast.Location.LocalTime);

        var result = await handler.Handle(new GetWeatherQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(expectedCount, result.Hourly.Count);
        Assert.True(result.Hourly.SequenceEqual(result.Hourly.OrderBy(item => item.Time)));

        var today = DateOnly.FromDateTime(forecast.Location.LocalTime.DateTime);
        var tomorrow = today.AddDays(1);
        var todayHours = result.Hourly
            .Where(item => DateOnly.FromDateTime(item.Time.DateTime) == today)
            .ToArray();
        var tomorrowHours = result.Hourly
            .Where(item => DateOnly.FromDateTime(item.Time.DateTime) == tomorrow)
            .ToArray();

        Assert.All(todayHours, item => Assert.True(item.Time >= forecast.Location.LocalTime));
        Assert.Equal(24, tomorrowHours.Length);
        Assert.Equal(Enumerable.Range(0, 24), tomorrowHours.Select(item => item.Time.Hour));
        Assert.All(
            result.Hourly,
            item => Assert.Contains(
                DateOnly.FromDateTime(item.Time.DateTime),
                new[] { today, tomorrow }));
    }

    [Fact]
    public async Task HandleUsesCurrentClockInsteadOfCachedProviderLocalTime()
    {
        var provider = Substitute.For<IWeatherProvider>();
        var forecast = CreateForecast(16, 15);
        provider.GetForecastAsync(Arg.Any<CancellationToken>()).Returns(forecast);
        var actualLocalNow = new DateTimeOffset(2026, 8, 10, 16, 40, 0, TimeSpan.FromHours(3));
        var handler = CreateHandler(provider, actualLocalNow);

        var result = await handler.Handle(new GetWeatherQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(actualLocalNow, result.Location.LocalTime);
        Assert.DoesNotContain(
            result.Hourly,
            item => item.Time == new DateTimeOffset(2026, 8, 10, 16, 0, 0, TimeSpan.FromHours(3)));
        Assert.Equal(
            new DateTimeOffset(2026, 8, 10, 17, 0, 0, TimeSpan.FromHours(3)),
            result.Hourly[0].Time);
    }

    [Fact]
    public async Task HandleReturnsExactlyThreeOrderedDays()
    {
        var provider = Substitute.For<IWeatherProvider>();
        var forecast = CreateForecast(12, 30);
        provider.GetForecastAsync(Arg.Any<CancellationToken>()).Returns(forecast);
        var handler = CreateHandler(provider, forecast.Location.LocalTime);

        var result = await handler.Handle(new GetWeatherQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Daily.Count);
        Assert.True(result.Daily.SequenceEqual(result.Daily.OrderBy(item => item.Date)));
    }

    [Fact]
    public async Task HandleRejectsForecastWithoutAllHoursTomorrow()
    {
        var provider = Substitute.For<IWeatherProvider>();
        var forecast = CreateForecast(12, 30);
        var incompleteForecast = forecast with
        {
            Hourly = forecast.Hourly
                .Where(item => item.Time != new DateTimeOffset(2026, 8, 11, 7, 0, 0, TimeSpan.FromHours(3)))
                .ToArray(),
        };
        provider.GetForecastAsync(Arg.Any<CancellationToken>()).Returns(incompleteForecast);
        var handler = CreateHandler(provider, forecast.Location.LocalTime);

        await Assert.ThrowsAsync<WeatherProviderUnavailableException>(
            () => handler.Handle(new GetWeatherQuery(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandlePassesCancellationTokenToProvider()
    {
        var provider = Substitute.For<IWeatherProvider>();
        var forecast = CreateForecast(12, 30);
        provider.GetForecastAsync(Arg.Any<CancellationToken>()).Returns(forecast);
        var handler = CreateHandler(provider, forecast.Location.LocalTime);
        using var cancellation = new CancellationTokenSource();

        await handler.Handle(new GetWeatherQuery(), cancellation.Token);

        await provider.Received(1).GetForecastAsync(cancellation.Token);
    }

    private static GetWeatherQueryHandler CreateHandler(
        IWeatherProvider provider,
        DateTimeOffset localNow) =>
        new(provider, new FixedTimeProvider(localNow.ToUniversalTime()));

    private static WeatherForecast CreateForecast(int hour, int minute)
    {
        var offset = TimeSpan.FromHours(3);
        var start = new DateTimeOffset(2026, 8, 10, 0, 0, 0, offset);
        var localNow = new DateTimeOffset(2026, 8, 10, hour, minute, 0, offset);
        var condition = new WeatherCondition(1003, "Partly cloudy", IsDay: true);

        var hourly = Enumerable.Range(0, 72)
            .Select(index => new HourlyWeather(
                start.AddHours(index),
                20 + (index % 8),
                21 + (index % 8),
                condition,
                index % 40,
                60,
                10))
            .ToArray();

        var daily = Enumerable.Range(0, 4)
            .Select(index => new DailyWeather(
                DateOnly.FromDateTime(start.DateTime).AddDays(index),
                12 + index,
                24 + index,
                18 + index,
                condition,
                20,
                18,
                65,
                4,
                "05:10 AM",
                "08:25 PM"))
            .Reverse()
            .ToArray();

        return new WeatherForecast(
            new WeatherLocation(
                "Moscow",
                "Moscow City",
                "Russia",
                "Europe/Moscow",
                55.75m,
                37.62m,
                localNow),
            new CurrentWeather(
                localNow,
                24,
                25,
                condition,
                12,
                "WSW",
                18,
                58,
                1013,
                0,
                10,
                4),
            hourly,
            daily);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
