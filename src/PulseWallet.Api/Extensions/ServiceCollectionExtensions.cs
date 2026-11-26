namespace PulseWallet.Api.Extensions;

/// <summary>
/// Service registration extensions for the Api (presentation) layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Api layer services to the container.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        return services;
    }
}
