using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PulseWallet.Domain.Common;
using IDiagnosticContext = Serilog.IDiagnosticContext;

namespace PulseWallet.Api.ExceptionHandling;

/// <summary>
/// Turns exceptions into problem details. A <see cref="PulseWalletException"/> sets its own status, title and detail.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IDiagnosticContext diagnosticContext,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            PulseWalletException expected => new ProblemDetails
            {
                Status = ToStatusCode(expected.ErrorType),
                Title = expected.Title,
                Detail = expected.Detail,
            },
            BadHttpRequestException badRequest => new ProblemDetails
            {
                Status = badRequest.StatusCode,
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = environment.IsDevelopment() ? exception.Message : null,
            },
        };

        int statusCode = problemDetails.Status!.Value;

        if (exception is PulseWalletException expectedException)
        {
            diagnosticContext.Set("ProblemTitle", problemDetails.Title);
            diagnosticContext.Set("ProblemDetail", problemDetails.Detail);
            foreach ((string name, object? value) in expectedException.Properties)
            {
                diagnosticContext.Set(name, value);
            }
        }
        else if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private static int ToStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception while processing {RequestMethod} {RequestPath}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string requestMethod, string requestPath);
}
