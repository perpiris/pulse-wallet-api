using System.Globalization;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace PulseWallet.Api.Logging;

/// <summary>
/// The console output shared by the bootstrap logger and the real one, so startup failures look like every other line.
/// </summary>
internal static class SerilogConsole
{
    /// <summary>
    /// Readable, colored lines in Development; one compact JSON object per line everywhere else.
    /// </summary>
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
