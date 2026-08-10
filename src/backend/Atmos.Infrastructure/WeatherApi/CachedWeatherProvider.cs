using Atmos.Application.Abstractions;
using Atmos.Application.Common.Exceptions;
using Atmos.Domain.Weather;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atmos.Infrastructure.WeatherApi;

public sealed class CachedWeatherProvider(
    WeatherApiClient innerProvider,
    IMemoryCache cache,
    IOptions<WeatherApiOptions> options,
    TimeProvider timeProvider,
    ILogger<CachedWeatherProvider> logger)
    : IWeatherProvider, IDisposable
{
    private const string CacheKey = "weather:moscow:forecast:v1";
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _disposed;

    public async Task<WeatherForecast> GetForecastAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (cache.TryGetValue(CacheKey, out WeatherForecast? cached) && cached is not null)
        {
            return cached;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var forecast = await innerProvider.GetForecastAsync(cancellationToken);
            var cacheDuration = GetCacheDuration(
                forecast.Location.TimeZoneId,
                options.Value.CacheDuration);

            cache.Set(
                CacheKey,
                forecast,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDuration,
                    Size = 1,
                });

            CachedWeatherProviderLog.ForecastCached(logger, cacheDuration);

            return forecast;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private TimeSpan GetCacheDuration(string timeZoneId, TimeSpan configuredDuration)
    {
        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
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

        var utcNow = timeProvider.GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var nextLocalDate = DateOnly.FromDateTime(localNow.DateTime).AddDays(1);
        var nextLocalMidnight = nextLocalDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocalMidnight, timeZone);
        var untilMidnight = new DateTimeOffset(nextMidnightUtc, TimeSpan.Zero) - utcNow;

        return untilMidnight < configuredDuration
            ? untilMidnight
            : configuredDuration;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _refreshLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

internal static partial class CachedWeatherProviderLog
{
    [LoggerMessage(
        EventId = 2100,
        Level = LogLevel.Debug,
        Message = "Cached weather forecast for {CacheDuration}")]
    public static partial void ForecastCached(ILogger logger, TimeSpan cacheDuration);
}
