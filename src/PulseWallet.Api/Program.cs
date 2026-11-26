using PulseWallet.Api.Extensions;
using PulseWallet.Api.Logging;
using PulseWallet.Application.Extensions;
using PulseWallet.Infrastructure.Extensions;
using Serilog;

// Logs startup failures until the real logger is built. The host environment isn't known yet, so read it the way
// the host will: ASPNETCORE_ENVIRONMENT, then DOTNET_ENVIRONMENT, and Production when neither is set.
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
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    WebApplication app = builder.Build();

    app.UseApi();

    app.Run();

    return 0;
}
catch (Exception exception)
{
    Log.Fatal(exception, "PulseWallet API terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
