using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atmos.Application.Common.Behaviors;

public sealed class RequestLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        RequestLoggingMessages.Handling(logger, requestName);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            RequestLoggingMessages.Handled(
                logger,
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch
        {
            stopwatch.Stop();
            RequestLoggingMessages.Failed(
                logger,
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

internal static partial class RequestLoggingMessages
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Handling {RequestName}")]
    public static partial void Handling(ILogger logger, string requestName);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Handled {RequestName} in {ElapsedMilliseconds} ms")]
    public static partial void Handled(
        ILogger logger,
        string requestName,
        long elapsedMilliseconds);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Failed {RequestName} after {ElapsedMilliseconds} ms")]
    public static partial void Failed(
        ILogger logger,
        string requestName,
        long elapsedMilliseconds);
}
