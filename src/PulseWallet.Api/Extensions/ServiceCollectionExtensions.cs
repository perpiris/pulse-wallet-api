using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PulseWallet.Api.ExceptionHandling;
using PulseWallet.Api.OpenApi;
using PulseWallet.Infrastructure.Identity;
using PulseWallet.Infrastructure.Persistence;

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
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            HttpContext httpContext = context.HttpContext;
            context.ProblemDetails.Instance ??= $"{httpContext.Request.Method} {httpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers();
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

        return services;
    }

    /// <summary>
    /// Identity with bearer tokens. Every endpoint requires a signed-in user unless marked [AllowAnonymous].
    /// </summary>
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        // Bearer tokens are ASP.NET Core's own opaque format (protected by Data Protection), not JWTs.
        services.AddIdentityApiEndpoints<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<PulseWalletDbContext>();

        services.Configure<IdentityOptions>(options => configuration.GetSection("Identity:Password").Bind(options.Password));

        return services;
    }

    public static IServiceCollection AddDatabaseHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<PulseWalletDbContext>("database");

        return services;
    }
}
