using Microsoft.Extensions.DependencyInjection;

namespace PulseWallet.Application.Extensions;

/// <summary>
/// Service registration extensions for the Application layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Application layer services to the container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
