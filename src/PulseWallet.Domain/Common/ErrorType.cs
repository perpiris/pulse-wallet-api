namespace PulseWallet.Domain.Common;

/// <summary>
/// What kind of failure a <see cref="PulseWalletException"/> represents. The Api layer maps it to an HTTP status code,
/// so the inner layers never depend on HTTP.
/// </summary>
public enum ErrorType
{
    /// <summary>The request itself is invalid (400).</summary>
    Validation,

    /// <summary>The requested resource does not exist (404).</summary>
    NotFound,

    /// <summary>The request conflicts with the current state, e.g. a duplicate (409).</summary>
    Conflict,

    /// <summary>The request is valid but breaks a business rule, e.g. insufficient funds (422).</summary>
    BusinessRule,

    /// <summary>A dependency the request needs is temporarily unavailable (503).</summary>
    Unavailable,
}
