using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PulseWallet.Infrastructure.Identity;

namespace PulseWallet.Infrastructure.Persistence;

/// <summary>
/// Business and Identity tables in one context, so business tables can have foreign keys to AspNetUsers.
/// </summary>
public class PulseWalletDbContext(DbContextOptions<PulseWalletDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Maps the Identity tables; must run before our own configurations.
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(PulseWalletDbContext).Assembly);
    }
}
