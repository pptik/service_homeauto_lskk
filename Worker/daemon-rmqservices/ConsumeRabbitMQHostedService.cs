namespace daemon_rmqservices
{
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;


    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;

        private static string RMQHost = ConfigurationManager.AppSettings["RMQHost"];
        private static string RMQVHost = ConfigurationManager.AppSettings["RMQVHost"];
        private static string RMQUsername = ConfigurationManager.AppSettings["RMQUsername"];
        private static string RMQPassword = ConfigurationManager.AppSettings["RMQPassword"];
        private static string RMQQueue = ConfigurationManager.AppSettings["RMQQueue"];
        private static string RMQSubRoutingKey = ConfigurationManager.AppSettings["RMQSubRoutingKey"];



        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory)
        {


            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {



            var factory = new ConnectionFactory { HostName = RMQHost, VirtualHost = RMQVHost, UserName = RMQUsername, Password = RMQPassword };

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            // _channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            // _channel.QueueDeclare("demo.queue.log", false, false, false, null);
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message
                var body = ea.Body.ToArray();
                var content = System.Text.Encoding.UTF8.GetString(body);

                // handle the received message
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            // consumer.Registered += OnConsumerRegistered;
            // consumer.Unregistered += OnConsumerUnregistered;
            // consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(RMQQueue, false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            // we just print this message 
            _logger.LogInformation($"consumer received {content}");
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

        // private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        // {
        //     _logger.LogInformation($"consumer cancelled {e.ConsumerTag}");
        // }

        // private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        // {
        //     _logger.LogInformation($"consumer unregistered {e.ConsumerTag}");
        // }

        // private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        // {
        //     _logger.LogInformation($"consumer registered {e.ConsumerTag}");
        // }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"consumer shutdown {e.ReplyText}");
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
