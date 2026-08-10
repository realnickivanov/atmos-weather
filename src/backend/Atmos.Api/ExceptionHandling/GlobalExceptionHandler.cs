using System.Diagnostics;
using Atmos.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Atmos.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            GlobalExceptionHandlerLog.ClientCanceledRequest(logger);
            return true;
        }

        var (statusCode, title, type) = exception switch
        {
            WeatherProviderUnavailableException => (
                StatusCodes.Status502BadGateway,
                "Weather service unavailable",
                "https://httpstatuses.com/502"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "https://httpstatuses.com/500"),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            GlobalExceptionHandlerLog.UnhandledException(logger, exception);
        }
        else
        {
            GlobalExceptionHandlerLog.DependencyFailure(
                logger,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = statusCode == StatusCodes.Status502BadGateway
                ? "The latest weather information could not be retrieved."
                : "The request could not be completed.",
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}

internal static partial class GlobalExceptionHandlerLog
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Debug,
        Message = "Request was canceled by the client")]
    public static partial void ClientCanceledRequest(ILogger logger);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "Unhandled exception while processing the request")]
    public static partial void UnhandledException(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Dependency failure while processing {Method} {Path}")]
    public static partial void DependencyFailure(
        ILogger logger,
        string method,
        PathString path,
        Exception exception);
}
