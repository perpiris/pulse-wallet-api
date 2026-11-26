using Serilog.Core;
using Serilog.Events;

namespace PulseWallet.Api.Logging;

/// <summary>
/// Adds " " + ProblemDetail, so the console template only gets a space on lines that have a detail.
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
