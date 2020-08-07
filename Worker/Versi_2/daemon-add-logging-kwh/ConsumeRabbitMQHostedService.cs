/*************************************************************************************************************************
 *                               DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                *
 *                                                VERSION             : 2                                                *
 *                                             TYPE APPLICATION    : WORKER                                              *
 * DESCRIPTION         : GET DATA FROM MQTT (OUTPUT DEVICE) CHECK TO DB RULES AND SEND BACK (INPUT DEVICE) IF DATA EXIST *
 *************************************************************************************************************************/

namespace daemon_add_logging_kwh
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
    using Newtonsoft.Json.Linq;



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
        private static string MessageSend = "";
        private static string serialNumberOutput = "";
        private static string valueOutput = "";
        private static string serialNumberInput = "";
        private static string valueInput = "";
        private static string StatusRegister = "Success Add Rule";


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
                
                _channel.BasicAck(ea.DeliveryTag, true);
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
            //_logger.LogInformation($"consumer received {content}");

            //And splite message to Query Parameters (NP: Income Data message must same with data structure)
            dynamic dataJson = JObject.Parse(content);
            serialNumberInput = dataJson.guidinput;
            valueInput = dataJson.valueinput;
            serialNumberOutput = dataJson.guidoutput;
            valueOutput = dataJson.valueoutput;

            //Open Connection Database
            connectionDB.Open();
            //Create command/query with param from mesage content  
            using (var transaction = connectionDB.BeginTransaction())
            {
                var insertCmd = connectionDB.CreateCommand();
                insertCmd.CommandText = "INSERT INTO kwh_logging (input_guid,input_value,output_guid,output_value) Values(@inputguid,@valueinput,@outputguid,@valueoutput)";
                insertCmd.Parameters.AddWithValue("@inputguid", serialNumberOutput);
                insertCmd.Parameters.AddWithValue("@valueinput", valueOutput);
                insertCmd.Parameters.AddWithValue("@outputguid", serialNumberInput);
                insertCmd.Parameters.AddWithValue("@valueoutput", valueInput);
                insertCmd.ExecuteNonQuery();
                transaction.Commit();

                MessageSend = StatusRegister;

                _channel.BasicPublish(
                exchange: RMQExc,
                routingKey: RMQPubRoutingKey,
                basicProperties: null,
                 body: Encoding.UTF8.GetBytes(MessageSend)

                );

            }
            connectionDB.Close();



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
