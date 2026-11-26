using System.Globalization;
using System.Text.RegularExpressions;

namespace PulseWallet.Domain.Common;

/// <summary>
/// Base class for the expected failures the API reports to its clients. Each subclass supplies a short title and a
/// message template with named placeholders; the API returns them as the problem details title and detail.
/// </summary>
/// <example>
/// <code>
/// public sealed class WalletNotFoundException(Guid walletId)
///     : PulseWalletException(ErrorType.NotFound, "Wallet not found", "Wallet {WalletId} was not found.", walletId);
/// </code>
/// </example>
public abstract partial class PulseWalletException : Exception
{
    /// <param name="errorType">What kind of failure this is; decides the HTTP status code.</param>
    /// <param name="title">A short, fixed summary of the problem type, e.g. "Wallet not found".</param>
    /// <param name="messageTemplate">
    /// The detail with named placeholders, e.g. "Wallet {WalletId} was not found.". Placeholders are filled from
    /// <paramref name="arguments"/> in order of appearance and may carry a format, e.g. "{Date:yyyy-MM-dd}".
    /// Use "{{" and "}}" for literal braces.
    /// </param>
    /// <param name="arguments">The placeholder values, in the order the placeholders appear.</param>
    protected PulseWalletException(ErrorType errorType, string title, string messageTemplate, params object?[] arguments)
        : this(errorType, title, messageTemplate, null, arguments)
    {
    }

    /// <summary>Same as the other constructor, plus the exception that caused this one.</summary>
    /// <param name="errorType">What kind of failure this is; decides the HTTP status code.</param>
    /// <param name="title">A short, fixed summary of the problem type.</param>
    /// <param name="messageTemplate">The detail with named placeholders.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    /// <param name="arguments">The placeholder values, in the order the placeholders appear.</param>
    protected PulseWalletException(
        ErrorType errorType,
        string title,
        string messageTemplate,
        Exception? innerException,
        params object?[] arguments)
        : this(errorType, title, messageTemplate, Render(messageTemplate, arguments), innerException)
    {
    }

    private PulseWalletException(
        ErrorType errorType,
        string title,
        string messageTemplate,
        (string Message, Dictionary<string, object?> Properties) rendered,
        Exception? innerException)
        : base(rendered.Message, innerException)
    {
        ErrorType = errorType;
        Title = title;
        MessageTemplate = messageTemplate;
        Properties = rendered.Properties;
    }

    /// <summary>What kind of failure this is.</summary>
    public ErrorType ErrorType { get; }

    /// <summary>A short, fixed summary of the problem type, returned as the problem details title.</summary>
    public string Title { get; }

    /// <summary>The unrendered template, e.g. "Wallet {WalletId} was not found.".</summary>
    public string MessageTemplate { get; }

    /// <summary>The placeholder values by name, e.g. WalletId → the id. Useful for structured logging.</summary>
    public IReadOnlyDictionary<string, object?> Properties { get; }

    /// <summary>The rendered template, returned as the problem details detail. Same as <see cref="Exception.Message"/>.</summary>
    public string Detail => Message;

    private static (string Message, Dictionary<string, object?> Properties) Render(string template, object?[] arguments)
    {
        Dictionary<string, object?> properties = [];
        int next = 0;

        string message = PlaceholderRegex().Replace(template, match =>
        {
            if (match.Value is "{{")
            {
                return "{";
            }

            if (match.Value is "}}")
            {
                return "}";
            }

            // More placeholders than arguments: keep the placeholder visible rather than throwing while
            // building an exception, which would hide the original failure.
            if (next >= arguments.Length)
            {
                return match.Value;
            }

            string name = match.Groups["name"].Value;
            object? value = arguments[next++];
            properties[name] = value;

            string? format = match.Groups["format"].Success ? match.Groups["format"].Value : null;
            return value switch
            {
                null => "null",
                IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty,
            };
        });

        return (message, properties);
    }

    [GeneratedRegex(@"\{\{|\}\}|\{(?<name>[A-Za-z_][A-Za-z0-9_]*)(?::(?<format>[^}]+))?\}")]
    private static partial Regex PlaceholderRegex();
}
