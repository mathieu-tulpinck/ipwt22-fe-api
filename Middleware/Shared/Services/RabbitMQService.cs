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

        public void InitializeBroker(ExchangeName exchangeName, QueueName queueName, RoutingKey routingKey)
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
                _channel.QueueDeclare(
                    queue: queueName.ToString(),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                _channel.QueueBind(
                    queue: queueName.ToString(),
                    exchange: exchangeName.ToString(),
                    routingKey: routingKey.ToString()
                );
            }
        }

        public void PublishMessage(ExchangeName exchangeName, RoutingKey routingkey, string xmlMessage)
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
                _channel.BasicPublish(
                    exchange: exchangeName.ToString(),
                    routingKey: routingkey.ToString(),
                    basicProperties: null,
                    body: body
                );
                try {
                    _channel.WaitForConfirmsOrDie(new TimeSpan(100)); // 5 seconds
                } catch (OperationInterruptedException e) {
                    _logger.LogError(e.Message);
                }
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