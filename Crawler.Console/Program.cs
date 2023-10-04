// See https://aka.ms/new-console-template for more information

using Crawler.Console;
using Crawler.Console.Abstraction;
using Crawler.Console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

const string appName = "Rozetka Spider";
Console.Title = appName;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Bootstrapping {Name}", appName);

Log.Information("Please, wait...");

var host = ConfigureHostServices(args)
    .Build();

try
{
    await host.RunAsync();

    Log.Information("Stopped cleanly");
}
catch (Exception exception)
{
    Log.Fatal(exception, "{Name} terminated unexpectedly", appName);
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

static IHostBuilder ConfigureHostServices(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console())
        .ConfigureServices((_, services) =>
        {
            var defaultPolicy = CustomRetryPolicy.GetDefault();
            services.AddHttpClient<IRozetkaApiClient, RozetkaApiClient>()
                .AddPolicyHandler(defaultPolicy);

            services.AddHttpClient<ICrawlerService, CrawlerService>()
                .AddPolicyHandler(defaultPolicy);

            services.AddHostedService<SpiderHostedService>();
        });
}