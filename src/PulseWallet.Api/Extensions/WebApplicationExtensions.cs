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
