using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PulseWallet.Domain.Common;
using IDiagnosticContext = Serilog.IDiagnosticContext;

namespace PulseWallet.Api.ExceptionHandling;

/// <summary>
/// Catches any unhandled exception, logs it and returns an RFC 9457 problem details response.
/// A <see cref="PulseWalletException"/> supplies its own title and detail; anything else is a 500.
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
                // Only expose the message while developing; never leak internals in production.
                Detail = environment.IsDevelopment() ? exception.Message : null,
            },
        };

        int statusCode = problemDetails.Status!.Value;

        if (exception is PulseWalletException expectedException)
        {
            // Expected failures are the client's problem: no stack trace, but the request log line
            // (UseSerilogRequestLogging) says why it failed instead of only the status code.
            diagnosticContext.Set("ProblemTitle", problemDetails.Title);
            diagnosticContext.Set("ProblemDetail", problemDetails.Detail);

            // The template values (WalletId, Requested, ...) as separate properties, so production logs can be
            // searched by them instead of by the rendered text.
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
