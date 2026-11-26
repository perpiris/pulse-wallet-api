using PulseWallet.Api.Extensions;
using PulseWallet.Application.Extensions;
using PulseWallet.Infrastructure.Extensions;

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApi();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    WebApplication app = builder.Build();

    app.UseApi();

    app.Run();

    return 0;
}
catch (Exception e)
{
    Console.WriteLine(e);
    return 1;
}
