using System.Globalization;
using System.Text.RegularExpressions;

namespace PulseWallet.Domain.Common;

/// <summary>
/// An expected failure. The API returns its title and rendered template as the problem details title and detail.
/// </summary>
/// <example>
/// <code>
/// public sealed class WalletNotFoundException(Guid walletId)
///     : PulseWalletException(ErrorType.NotFound, "Wallet not found", "Wallet {WalletId} was not found.", walletId);
/// </code>
/// </example>
public abstract partial class PulseWalletException : Exception
{
    /// <param name="errorType">Decides the HTTP status code.</param>
    /// <param name="title">Fixed per exception type, e.g. "Wallet not found".</param>
    /// <param name="messageTemplate">
    /// E.g. "Wallet {WalletId} was not found.". Filled from <paramref name="arguments"/> in order; formats like
    /// "{Date:yyyy-MM-dd}" work, and "{{" / "}}" are literal braces.
    /// </param>
    /// <param name="arguments">The placeholder values, in order.</param>
    protected PulseWalletException(ErrorType errorType, string title, string messageTemplate, params object?[] arguments)
        : this(errorType, title, messageTemplate, null, arguments)
    {
    }

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

    public ErrorType ErrorType { get; }

    public string Title { get; }

    public string MessageTemplate { get; }

    /// <summary>Placeholder values by name, for structured logging.</summary>
    public IReadOnlyDictionary<string, object?> Properties { get; }

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

            // Don't throw while building an exception: it would hide the original failure.
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
