using Middleware.Shared.Enums;
using Middleware.Shared.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Middleware.Shared.Messages;
using Serilog;
using System.Text;

namespace Middleware.Shared.Services
{
    public class EventBus : IDisposable
    {
        private readonly RabbitMQPersistentConnection _rbmqConnection;
        private IModel? _consumerChannel;
        private string _queueName;
        private readonly XMLService _xmlService;

        public EventBus(RabbitMQPersistentConnection rbmqConnection, ConsumingQueueName queueName)
        {
            _rbmqConnection = rbmqConnection ?? throw new ArgumentNullException(nameof(rbmqConnection));
            _queueName = queueName.ToString();
            var factory = LoggerFactory.Create(b => b.AddConsole());
            _xmlService = new XMLService(factory.CreateLogger<XMLService>());
        }

        public IModel CreateConsumerChannel()
        {
            if (!_rbmqConnection.IsConnected)
            {
                _rbmqConnection.TryConnect();
            }
            var channel = _rbmqConnection.CreateModel();
            channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ReceivedEvent;
            channel.BasicConsume(
                queue: _queueName,
                autoAck: true,
                consumer: consumer
            );
            channel.CallbackException += (sender, ea) =>
            {
                if (_consumerChannel is not null) {
                    _consumerChannel.Dispose();
                }
                _consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }

        private void ReceivedEvent(object? sender, BasicDeliverEventArgs e)
        {
            Log.Information("Message received.");
            var xmlMessage = Encoding.UTF8.GetString(e.Body.Span);
            Log.Information(xmlMessage);
            switch(Enum.Parse<RoutingKey>(e.RoutingKey)) {
                case RoutingKey.FrontSession:
                    Log.Information("case selected");
                    if (_xmlService.ValidateXml(xmlMessage, "SessionEvent_w.xsd")) {
                        Log.Information("XSD validation passed.");
                        var sessionEventMessage = new SessionEventMessage();
                        _xmlService.DeserializeFromXML<SessionEventMessage>(xmlMessage, out sessionEventMessage);
                    
                        Log.Information(sessionEventMessage.ToString());
                    }

                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            if (_consumerChannel != null) {
                _consumerChannel.Dispose();
            }
        }
    }
}