using Serilog.Core;
using Serilog.Events;

namespace PulseWallet.Api.Logging;

/// <summary>
/// Adds "ProblemDetailSuffix" (a space plus the ProblemDetail) to events that have a ProblemDetail, so the console
/// template can append the failure reason to a line without leaving a trailing space on every other line.
/// </summary>
internal sealed class ProblemDetailSuffixEnricher : ILogEventEnricher
{
    public const string PropertyName = "ProblemDetailSuffix";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("ProblemDetail", out LogEventPropertyValue? value)
            && value is ScalarValue { Value: string detail }
            && detail.Length > 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(PropertyName, " " + detail));
        }
    }
}
