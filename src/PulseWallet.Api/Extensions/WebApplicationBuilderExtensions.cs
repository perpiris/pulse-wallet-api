using PulseWallet.Api.Logging;
using Serilog;

namespace PulseWallet.Api.Extensions;

/// <summary>
/// Host-level configuration extensions for the Api (presentation) layer.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((serviceProvider, config) => config
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext()
            .WriteToConsole(builder.Environment.IsDevelopment()));

        return builder;
    }
}
