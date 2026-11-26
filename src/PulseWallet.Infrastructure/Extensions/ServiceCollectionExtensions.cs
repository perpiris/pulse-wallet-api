using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}
