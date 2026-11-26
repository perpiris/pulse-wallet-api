using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseWallet.Infrastructure.Persistence;

namespace PulseWallet.Infrastructure.Extensions;

/// <summary>
/// Service registration extensions for the Infrastructure layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Infrastructure layer services to the container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Docker: ConnectionStrings__PulseWallet in compose.yaml. IDE: user secrets. Never appsettings.
        string connectionString = configuration.GetConnectionString("PulseWallet")
            ?? throw new InvalidOperationException("Connection string 'PulseWallet' is not configured.");

        // With retries on, a transaction opened by hand must run inside Database.CreateExecutionStrategy().
        services.AddDbContext<PulseWalletDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure()));

        return services;
    }
}
