using System.Globalization;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace PulseWallet.Api.Logging;

/// <summary>
/// Shared by the bootstrap logger and the real one, so startup failures look like every other line.
/// </summary>
internal static class SerilogConsole
{
    public static LoggerConfiguration WriteToConsole(this LoggerConfiguration config, bool isDevelopment)
    {
        if (!isDevelopment)
        {
            return config.WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        return config
            .Enrich.With<ProblemDetailSuffixEnricher>()
            .WriteTo.Console(
                outputTemplate: $"[{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{{ProblemDetailSuffixEnricher.PropertyName}:l}}{{NewLine}}{{Exception}}",
                formatProvider: CultureInfo.InvariantCulture,
                theme: AnsiConsoleTheme.Code);
    }
}
