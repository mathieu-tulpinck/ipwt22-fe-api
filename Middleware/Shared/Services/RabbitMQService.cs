using Microsoft.Extensions.Logging;
using Middleware.Shared.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace Middleware.Shared.Services
{
    public class RabbitMQService
    {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly RabbitMQPersistentConnection _rbmqConnection;
        private IModel? _channel;

        public RabbitMQService(ILogger<RabbitMQService> logger, RabbitMQPersistentConnection rbmqConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rbmqConnection = rbmqConnection ?? throw new ArgumentNullException(nameof(rbmqConnection));    
        }

        public void ConfigureBroker(ExchangeName exchangeName, Dictionary<QueueName, RoutingKey> bindings)
        {
            if (!_rbmqConnection.IsConnected) {
                _rbmqConnection.TryConnect();
            }
            _channel = _rbmqConnection.CreateModel();
            using (_channel) {
                _channel.ExchangeDeclare(
                    exchange: exchangeName.ToString(),
                    type: "direct"
                );
                foreach (var binding in bindings) {
                    _channel.QueueDeclare(
                    queue: binding.Key.ToString(),
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );
                     _channel.QueueBind(
                    queue: binding.Key.ToString(),
                    exchange: exchangeName.ToString(),
                    routingKey: binding.Value.ToString() // Binding key.
                    );
                }
            }
        }

        public void PublishMessage(ExchangeName exchangeName, Dictionary<QueueName, RoutingKey>.ValueCollection routingkeys, string xmlMessage)
        {
            if (!_rbmqConnection.IsConnected) {
                _rbmqConnection.TryConnect();
            }
            _channel = _rbmqConnection.CreateModel();

            using (_channel) {
                _channel.ConfirmSelect();
                _channel.BasicAcks += (sender, ea) => {
                    _logger.LogInformation("The RabbitMQ broker ack'ed the message.");
                };
                var body = Encoding.UTF8.GetBytes(xmlMessage);
                foreach (var routingKey in routingkeys) {
                    _channel.BasicPublish(
                    exchange: exchangeName.ToString(),
                    routingKey: routingKey.ToString(), // Routing key.
                    basicProperties: null,
                    body: body
                    );
                }
                // try {
                //     _channel.WaitForConfirmsOrDie(new TimeSpan(100)); // 5 seconds
                // } catch (OperationInterruptedException e) {
                //     _logger.LogError(e.Message);
                // }

            }
        }

        public void Dispose()
        {
            if (_channel is not null) {
                _channel.Dispose();
            }
        }
    }
}