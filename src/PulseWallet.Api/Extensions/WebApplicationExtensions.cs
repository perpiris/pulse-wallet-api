using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PulseWallet.Infrastructure.Identity;
using PulseWallet.Infrastructure.Persistence;
using Serilog;
using Serilog.Events;

namespace PulseWallet.Api.Extensions;

/// <summary>
/// Request pipeline extensions for the Api (presentation) layer.
/// </summary>
public static class WebApplicationExtensions
{
    private const string HealthPath = "/health";

    /// <summary>
    /// Configures the Api middleware pipeline and endpoints.
    /// </summary>
    public static WebApplication UseApi(this WebApplication app)
    {
        // Before the exception handler, so it logs the final status code.
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = GetRequestLogLevel;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Behind a reverse proxy this is the proxy's address until forwarded headers are configured.
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
            };
        });
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/openapi/v1.json", "PulseWallet API v1"));
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapAuthEndpoints();
        app.MapHealthChecks(HealthPath).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Development only. In production, instances would race and the app shouldn't have rights to change the schema.
    /// </summary>
    public static async Task ApplyMigrationsInDevelopmentAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        PulseWalletDbContext dbContext = scope.ServiceProvider.GetRequiredService<PulseWalletDbContext>();
        await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }

    private static void MapAuthEndpoints(this WebApplication app)
    {
        IEndpointConventionBuilder identityEndpoints = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .MapIdentityApi<ApplicationUser>();

        // The fallback policy would also lock register, login, refresh and password reset; only /manage/* needs sign-in.
        identityEndpoints.Add(endpoint =>
        {
            if (endpoint is RouteEndpointBuilder { RoutePattern.RawText: string pattern }
                && !pattern.Contains("/manage", StringComparison.Ordinal))
            {
                endpoint.Metadata.Add(new AllowAnonymousAttribute());
            }
        });
    }

    private static LogEventLevel GetRequestLogLevel(HttpContext httpContext, double elapsedMilliseconds, Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        // Verbose is below the minimum level, so successful probes never reach the logs.
        return httpContext.Request.Path.StartsWithSegments(HealthPath)
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
    }
}
