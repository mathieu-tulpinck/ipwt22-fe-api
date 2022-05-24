using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Middleware.Shared.Services;
using RabbitMQ.Client;
using Serilog;

using var log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();
Log.Logger = log;

var host = CreateHostBuilder(args).Build();
Application app = host.Services.GetRequiredService<Application>();
app.Run();

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) => services
            .AddSingleton<Application>()
            .AddSingleton<RabbitMQPersistentConnection>(sp => {
                var logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory() { HostName = "rabbitmq" };

                return new RabbitMQPersistentConnection(logger, factory);
            })
            // .AddHttpClient("UuidMasterApi", httpClient => {
            //     httpClient.BaseAddress = new Uri("http://api:5000/api");
            //     httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            // })
        );
}

