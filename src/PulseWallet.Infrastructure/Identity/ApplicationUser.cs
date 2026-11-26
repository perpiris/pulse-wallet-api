using Microsoft.AspNetCore.Identity;

namespace PulseWallet.Infrastructure.Identity;

/// <summary>
/// The sign-in account.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
