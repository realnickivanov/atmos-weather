using System.Net.Http.Headers;
using System.Text.Json;
using Atmos.Application.Abstractions;
using Atmos.Application.Common.Exceptions;
using Atmos.Domain.Weather;
using Atmos.Infrastructure.WeatherApi.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Atmos.Infrastructure.WeatherApi;

public sealed class WeatherApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<WeatherApiOptions> options,
    ILogger<WeatherApiClient> logger)
    : IWeatherProvider
{
    public const string HttpClientName = "WeatherApi";

    private readonly WeatherApiOptions _options = options.Value;

    public async Task<WeatherForecast> GetForecastAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);
        var requestUri = BuildRequestUri();

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                WeatherApiClientLog.ProviderReturnedFailure(
                    logger,
                    (int)response.StatusCode);

                throw new WeatherProviderUnavailableException(
                    "The weather provider is temporarily unavailable.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync(
                stream,
                WeatherApiJsonContext.Default.WeatherApiForecastResponse,
                cancellationToken);

            if (payload is null)
            {
                throw new WeatherProviderUnavailableException(
                    "The weather provider returned an empty response.");
            }

            return WeatherApiMapper.Map(payload);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            WeatherApiClientLog.RequestTimedOut(logger, exception);
            throw new WeatherProviderUnavailableException(
                "The weather provider request timed out.",
                exception);
        }
        catch (ExecutionRejectedException exception)
        {
            WeatherApiClientLog.RequestRejected(logger, exception);
            throw new WeatherProviderUnavailableException(
                "The weather provider is temporarily unavailable.",
                exception);
        }
        catch (WeatherProviderUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            WeatherApiClientLog.RequestFailed(logger, exception);
            throw new WeatherProviderUnavailableException(
                "The weather provider could not be reached.",
                exception);
        }
        catch (JsonException exception)
        {
            WeatherApiClientLog.InvalidJson(logger, exception);
            throw new WeatherProviderUnavailableException(
                "The weather provider returned an invalid response.",
                exception);
        }
    }

    private string BuildRequestUri()
    {
        var key = Uri.EscapeDataString(_options.ApiKey);
        var location = Uri.EscapeDataString(_options.LocationQuery);

        return $"forecast.json?key={key}&q={location}&days={_options.ForecastDays}&aqi=no&alerts=no";
    }
}

internal static partial class WeatherApiClientLog
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Warning,
        Message = "Weather provider returned HTTP status {StatusCode}")]
    public static partial void ProviderReturnedFailure(ILogger logger, int statusCode);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Weather provider request timed out")]
    public static partial void RequestTimedOut(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Weather provider request was rejected by the resilience pipeline")]
    public static partial void RequestRejected(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Weather provider request failed")]
    public static partial void RequestFailed(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Weather provider returned invalid JSON")]
    public static partial void InvalidJson(ILogger logger, Exception exception);
}
