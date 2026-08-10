using MediatR;

namespace Atmos.Application.Weather.GetWeather;

public sealed record GetWeatherQuery : IRequest<WeatherResponse>;
