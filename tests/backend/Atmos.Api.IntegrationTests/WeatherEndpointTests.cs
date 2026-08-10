using System.Net;
using System.Net.Http.Json;
using Atmos.Application.Common.Exceptions;
using Atmos.Application.Weather.GetWeather;
using Microsoft.AspNetCore.Mvc;

namespace Atmos.Api.IntegrationTests;

public sealed class WeatherEndpointTests
{
    [Fact]
    public async Task GetWeatherReturnsExpectedContract()
    {
        await using var factory = new CustomWebApplicationFactory(
            CustomWebApplicationFactory.CreateSuccessfulProvider());
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/weather",
            TestContext.Current.CancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<WeatherResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Moscow", payload.Location.Name);
        Assert.Equal(3, payload.Daily.Count);
        Assert.Equal(31, payload.Hourly.Count);
    }

    [Fact]
    public async Task GetWeatherReturnsProblemDetailsWhenProviderFails()
    {
        await using var factory = new CustomWebApplicationFactory(
            CustomWebApplicationFactory.CreateFailingProvider(
                new WeatherProviderUnavailableException("Provider unavailable")));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/weather",
            TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Weather service unavailable", problem.Title);
        Assert.Equal((int)HttpStatusCode.BadGateway, problem.Status);
    }

    [Fact]
    public async Task HealthReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory(
            CustomWebApplicationFactory.CreateSuccessfulProvider());
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/health",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
