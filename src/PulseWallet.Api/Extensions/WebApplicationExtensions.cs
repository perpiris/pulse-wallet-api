using System.Security.Claims;
using Serilog;

namespace PulseWallet.Api.Extensions;

/// <summary>
/// Request pipeline extensions for the Api (presentation) layer.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the Api middleware pipeline and endpoints.
    /// </summary>
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            // Behind a reverse proxy this is the proxy's address until forwarded headers are configured.
            diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
        });
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/openapi/v1.json", "PulseWallet API v1"));
        }

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
