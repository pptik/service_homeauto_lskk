/*************************************************************************************************************************
 *                               DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                *
 *                                                VERSION             : 2                                                *
 *                                             TYPE APPLICATION    : WORKER                                              *
 * DESCRIPTION         : GET DATA FROM MQTT (OUTPUT DEVICE) CHECK TO DB RULES AND SEND BACK (INPUT DEVICE) IF DATA EXIST *
 *************************************************************************************************************************/

namespace daemon_rmqservices
{
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Microsoft.Data.Sqlite;



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
        private static string RMQExc = ConfigurationManager.AppSettings["RMQExc"];
        private static string RMQPubRoutingKey = ConfigurationManager.AppSettings["RMQPubRoutingKey"];
        private static string DBPath = ConfigurationManager.AppSettings["DBPath"];
        private static string InputGuid = "";
        private static string ValueInput = "";
        private static string OutputGuid = "";
        private static string ValueOutput = "";
        private static string MessageSend = "";



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
                HandleMessageToDB(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;

            _channel.BasicConsume(RMQQueue, false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessageToDB(string content)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = DBPath;
            var connectionDB = new SqliteConnection(connectionStringBuilder.ConnectionString);

            //just print this message 
             _logger.LogInformation($"consumer received {content}");

            //And splite message to Query Parameters

            string[] dataParsing = content.Split('#');
            foreach (var datas in dataParsing)
            {
                //System.Console.WriteLine($"{datas}>");
                InputGuid = dataParsing[0];
                ValueInput = dataParsing[1];

            }
            connectionDB.Open();
            var selectCmd = connectionDB.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM activityiot  WHERE input_guid=@Guidinput AND input_value=@Valueinput";
            selectCmd.Parameters.AddWithValue("@Guidinput", InputGuid);
            selectCmd.Parameters.AddWithValue("@Valueinput", ValueInput);
            using (var reader = selectCmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    OutputGuid = reader.GetString(3);
                    ValueOutput = reader.GetString(4);


                    MessageSend = OutputGuid + "#" + ValueOutput;

                    _channel.BasicPublish(exchange: RMQExc,
                                            routingKey: RMQPubRoutingKey,
                                            basicProperties: null,
                                            body: Encoding.UTF8.GetBytes(MessageSend)
                    );

                   

                }

                connectionDB.Close();
            }
             _logger.LogInformation("Sucess Send Data");


        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

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
