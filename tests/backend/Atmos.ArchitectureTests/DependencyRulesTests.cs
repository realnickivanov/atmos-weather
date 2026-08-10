using System.Reflection;
using Atmos.Application.Abstractions;
using Atmos.Domain.Weather;
using Atmos.Infrastructure.WeatherApi;

namespace Atmos.ArchitectureTests;

public sealed class DependencyRulesTests
{
    [Fact]
    public void DomainHasNoDependencyOnOuterLayers()
    {
        var references = GetReferences(typeof(WeatherForecast).Assembly);

        Assert.DoesNotContain("Atmos.Application", references);
        Assert.DoesNotContain("Atmos.Infrastructure", references);
        Assert.DoesNotContain("Atmos.Api", references);
    }

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructureOrApi()
    {
        var references = GetReferences(typeof(IWeatherProvider).Assembly);

        Assert.Contains("Atmos.Domain", references);
        Assert.DoesNotContain("Atmos.Infrastructure", references);
        Assert.DoesNotContain("Atmos.Api", references);
    }

    [Fact]
    public void InfrastructureDoesNotDependOnApi()
    {
        var references = GetReferences(typeof(WeatherApiClient).Assembly);

        Assert.Contains("Atmos.Application", references);
        Assert.Contains("Atmos.Domain", references);
        Assert.DoesNotContain("Atmos.Api", references);
    }

    private static HashSet<string> GetReferences(Assembly assembly) =>
        assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);
}
