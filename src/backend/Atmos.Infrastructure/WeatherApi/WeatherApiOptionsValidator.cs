using Microsoft.Extensions.Options;

namespace Atmos.Infrastructure.WeatherApi;

internal sealed class WeatherApiOptionsValidator : IValidateOptions<WeatherApiOptions>
{
    public ValidateOptionsResult Validate(string? name, WeatherApiOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add("WeatherApi:BaseUrl must be an absolute HTTPS URL.");
        }
        else if (!options.BaseUrl.EndsWith('/'))
        {
            failures.Add("WeatherApi:BaseUrl must end with a forward slash.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add("WeatherApi:ApiKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.LocationQuery))
        {
            failures.Add("WeatherApi:LocationQuery is required.");
        }

        if (options.ForecastDays != 3)
        {
            failures.Add("WeatherApi:ForecastDays must be 3 for this application.");
        }

        if (options.CacheDuration <= TimeSpan.Zero
            || options.CacheDuration > TimeSpan.FromHours(1))
        {
            failures.Add("WeatherApi:CacheDuration must be greater than zero and no more than one hour.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
