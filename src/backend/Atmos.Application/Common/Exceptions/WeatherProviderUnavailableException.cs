namespace Atmos.Application.Common.Exceptions;

public sealed class WeatherProviderUnavailableException : Exception
{
    public WeatherProviderUnavailableException()
    {
    }

    public WeatherProviderUnavailableException(string message)
        : base(message)
    {
    }

    public WeatherProviderUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
