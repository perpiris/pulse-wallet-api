using PulseWallet.Api.Extensions;
using PulseWallet.Api.Logging;
using PulseWallet.Application.Extensions;
using PulseWallet.Infrastructure.Extensions;
using Serilog;

string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environments.Production;

Log.Logger = new LoggerConfiguration()
    .WriteToConsole(string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging();

    builder.Services.AddApi();
    builder.Services.AddIdentityAuthentication(builder.Configuration);
    builder.Services.AddDatabaseHealthChecks();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    WebApplication app = builder.Build();

    await app.ApplyMigrationsInDevelopmentAsync();

    app.UseApi();

    await app.RunAsync();

    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "PulseWallet API terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
