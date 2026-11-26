namespace PulseWallet.Domain.Common;

/// <summary>
/// Mapped to an HTTP status code by the Api layer, so the inner layers don't depend on HTTP.
/// </summary>
public enum ErrorType
{
    /// <summary>400: the request is invalid.</summary>
    Validation,

    /// <summary>404: the resource doesn't exist.</summary>
    NotFound,

    /// <summary>409: conflicts with the current state, e.g. a duplicate.</summary>
    Conflict,

    /// <summary>422: valid, but breaks a business rule, e.g. insufficient funds.</summary>
    BusinessRule,

    /// <summary>503: a dependency is temporarily unavailable.</summary>
    Unavailable,
}
