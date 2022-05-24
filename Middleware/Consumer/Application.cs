using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Middleware.Shared.Services;

public class Application
{
    public static RabbitMQPersistentConnection? Listener;
    private readonly ILogger<Application> _logger;

    public Application(ILogger<Application> logger, RabbitMQPersistentConnection rbmqConnection)
    {
        Listener = rbmqConnection ?? throw new ArgumentNullException(nameof(rbmqConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Run()
    {
        _logger.LogInformation("Starting consumer");
        Listener!.CreateConsumerChannel();
        Thread.Sleep(Timeout.Infinite); // Very hacky solution. To be replaced. IHostedService / BackgroundService.
    }
}