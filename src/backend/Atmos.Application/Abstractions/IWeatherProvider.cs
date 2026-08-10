using Atmos.Domain.Weather;

namespace Atmos.Application.Abstractions;

public interface IWeatherProvider
{
    Task<WeatherForecast> GetForecastAsync(CancellationToken cancellationToken);
}
