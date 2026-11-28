namespace PulseWallet.Domain.Common;

/// <summary>
/// A simple input rule was broken. For failures the client handles differently, create a dedicated exception.
/// </summary>
public sealed class DomainValidationException(string messageTemplate, params object?[] arguments)
    : PulseWalletException(ErrorType.Validation, "Invalid request", messageTemplate, arguments)
{
    public override string Code => "ValidationFailed";
}
