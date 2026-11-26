using System.Diagnostics;
using PulseWallet.Api.ExceptionHandling;

namespace PulseWallet.Api.Extensions;

/// <summary>
/// Service registration extensions for the Api (presentation) layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Api layer services to the container.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            HttpContext httpContext = context.HttpContext;
            context.ProblemDetails.Instance ??= $"{httpContext.Request.Method} {httpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers();
        services.AddOpenApi();

        return services;
    }
}
