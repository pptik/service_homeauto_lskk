
/*************************************************************************************************************************
 *                               DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                *
 *                                                VERSION             : 2                                                *
 *                                             TYPE APPLICATION    : WORKER                                              *
 * DESCRIPTION         : GET DATA FROM MQTT (OUTPUT DEVICE) CHECK TO DB RULES AND SEND BACK (INPUT DEVICE) IF DATA EXIST *
 *************************************************************************************************************************/

namespace daemon_rmqservices_sensor
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
    using System;



    public class ConsumeRabbitMQHostedServiceSensor : BackgroundService
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
        private static string serialNumber = "";
        private static string deviceName = "";
        private static string valueInput = "";




        public ConsumeRabbitMQHostedServiceSensor(ILoggerFactory loggerFactory)
        {

            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedServiceSensor>();
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

            _channel.BasicConsume(RMQQueue, true, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessageToDB(string content)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = DBPath;
            var connectionDB = new SqliteConnection(connectionStringBuilder.ConnectionString);

            //just print this message 
            _logger.LogInformation($"consumer received {content}");

            //And splite message to Query Parameters (NP: Income Data message must same with data structure)

            string[] dataParsing = content.Split('#');
            foreach (var datas in dataParsing)
            {
                //System.Console.WriteLine($"{datas}>");
                serialNumber = dataParsing[0];
                deviceName = dataParsing[1];
                valueInput = dataParsing[2];


            }

            DateTime now = DateTime.Now;
            String timeStamp = now.ToString();
            _logger.LogInformation($"consumer received guid {serialNumber}");
            _logger.LogInformation($"consumer received device name {deviceName}");
            _logger.LogInformation($"consumer received value {valueInput}");


            //Open Connection Database
            connectionDB.Open();
            //Create command/query with param from mesage content  
            using (var transaction = connectionDB.BeginTransaction())
            {
                var insertCmd = connectionDB.CreateCommand();
                insertCmd.CommandText = "INSERT INTO logsecurity (input_guid,name_device,value_device,time_stamp) Values(@inputguid,@devicename,@valueinput,@timestamp)";
                insertCmd.Parameters.AddWithValue("@inputguid", serialNumber);
                insertCmd.Parameters.AddWithValue("@devicename", deviceName);
                insertCmd.Parameters.AddWithValue("@valueoutput", valueInput);
                insertCmd.Parameters.AddWithValue("@timestamp", timeStamp);
                insertCmd.ExecuteNonQuery();
                transaction.Commit();

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
