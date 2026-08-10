using System.Net;
using System.Text.Json.Nodes;
using Atmos.Application.Common.Exceptions;
using Atmos.Infrastructure.WeatherApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Atmos.Infrastructure.Tests;

public sealed class WeatherApiClientTests
{
    [Fact]
    public async Task GetForecastAsyncUsesSingleForecastEndpointAndMapsResponse()
    {
        Uri? capturedRequestUri = null;
        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "weather-api-forecast.json"),
            TestContext.Current.CancellationToken);
        var handler = new StubHttpMessageHandler(request =>
        {
            capturedRequestUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json),
            };
        });
        var client = CreateClient(handler);

        var result = await client.GetForecastAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(capturedRequestUri);
        Assert.EndsWith("/forecast.json", capturedRequestUri.AbsolutePath, StringComparison.Ordinal);
        Assert.Contains("days=3", capturedRequestUri.Query, StringComparison.Ordinal);
        Assert.Contains("q=55.7558%2C37.6173", capturedRequestUri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("current.json", capturedRequestUri.ToString(), StringComparison.Ordinal);
        Assert.Equal("Moscow", result.Location.Name);
        Assert.Equal(3, result.Daily.Count);
        Assert.Equal(72, result.Hourly.Count);
        Assert.Equal(1003, result.Current.Condition.Code);
    }

    [Fact]
    public async Task GetForecastAsyncThrowsProviderExceptionForNonSuccessStatus()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<WeatherProviderUnavailableException>(() =>
            client.GetForecastAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetForecastAsyncRejectsIncompleteHourlyForecast()
    {
        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "weather-api-forecast.json"),
            TestContext.Current.CancellationToken);
        var root = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("Fixture JSON could not be parsed.");
        var hours = root["forecast"]?["forecastday"]?[1]?["hour"]?.AsArray()
            ?? throw new InvalidOperationException("Fixture hourly data could not be found.");
        hours.RemoveAt(hours.Count - 1);

        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(root.ToJsonString()),
            });
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<WeatherProviderUnavailableException>(() =>
            client.GetForecastAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetForecastAsyncRejectsAverageOutsideDailyTemperatureRange()
    {
        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "weather-api-forecast.json"),
            TestContext.Current.CancellationToken);
        var root = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("Fixture JSON could not be parsed.");
        var day = root["forecast"]?["forecastday"]?[0]?["day"]?.AsObject()
            ?? throw new InvalidOperationException("Fixture daily data could not be found.");
        day["avgtemp_c"] = 99;

        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(root.ToJsonString()),
            });
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<WeatherProviderUnavailableException>(() =>
            client.GetForecastAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CachedProviderReusesForecastWithinCacheWindow()
    {
        var requestCount = 0;
        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "weather-api-forecast.json"),
            TestContext.Current.CancellationToken);
        var handler = new StubHttpMessageHandler(_ =>
        {
            requestCount++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json),
            };
        });
        var options = CreateOptions();
        var client = CreateClient(handler, options);
        using var cache = new MemoryCache(
            new MemoryCacheOptions { SizeLimit = 16 });
        using var provider = new CachedWeatherProvider(
            client,
            cache,
            options,
            TimeProvider.System,
            NullLogger<CachedWeatherProvider>.Instance);

        var first = await provider.GetForecastAsync(TestContext.Current.CancellationToken);
        var second = await provider.GetForecastAsync(TestContext.Current.CancellationToken);

        Assert.Same(first, second);
        Assert.Equal(1, requestCount);
    }

    [Fact]
    public async Task CachedProviderCapsCacheAtLocationMidnight()
    {
        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "weather-api-forecast.json"),
            TestContext.Current.CancellationToken);
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json),
            });
        var options = CreateOptions();
        var client = CreateClient(handler, options);
        using var cache = new CapturingMemoryCache();
        var utcNow = new DateTimeOffset(2026, 8, 10, 20, 58, 0, TimeSpan.Zero);
        using var provider = new CachedWeatherProvider(
            client,
            cache,
            options,
            new FixedTimeProvider(utcNow),
            NullLogger<CachedWeatherProvider>.Instance);

        await provider.GetForecastAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TimeSpan.FromMinutes(2), cache.AbsoluteExpirationRelativeToNow);
    }

    private static WeatherApiClient CreateClient(
        HttpMessageHandler handler,
        IOptions<WeatherApiOptions>? options = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.weatherapi.com/v1/"),
        };
        var factory = new StubHttpClientFactory(httpClient);

        return new WeatherApiClient(
            factory,
            options ?? CreateOptions(),
            NullLogger<WeatherApiClient>.Instance);
    }

    private static IOptions<WeatherApiOptions> CreateOptions() =>
        Options.Create(new WeatherApiOptions
        {
            ApiKey = "test-key",
            LocationQuery = "55.7558,37.6173",
            ForecastDays = 3,
            CacheDuration = TimeSpan.FromMinutes(5),
        });

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class CapturingMemoryCache : IMemoryCache
    {
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; private set; }

        public bool TryGetValue(object key, out object? value)
        {
            value = null;
            return false;
        }

        public ICacheEntry CreateEntry(object key) => new CapturingCacheEntry(key, this);

        public void Remove(object key)
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private sealed class CapturingCacheEntry(
            object key,
            CapturingMemoryCache owner) : ICacheEntry
        {
            public object Key { get; } = key;

            public object? Value { get; set; }

            public DateTimeOffset? AbsoluteExpiration { get; set; }

            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

            public TimeSpan? SlidingExpiration { get; set; }

            public IList<Microsoft.Extensions.Primitives.IChangeToken> ExpirationTokens { get; } =
                new List<Microsoft.Extensions.Primitives.IChangeToken>();

            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } =
                new List<PostEvictionCallbackRegistration>();

            public CacheItemPriority Priority { get; set; }

            public long? Size { get; set; }

            public void Dispose()
            {
                owner.AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow;
                GC.SuppressFinalize(this);
            }
        }
    }
}
