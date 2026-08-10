using System.Text.Json.Serialization;
using Atmos.Infrastructure.WeatherApi.Models;

namespace Atmos.Infrastructure.WeatherApi.Serialization;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = false,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(WeatherApiForecastResponse))]
internal sealed partial class WeatherApiJsonContext : JsonSerializerContext;
