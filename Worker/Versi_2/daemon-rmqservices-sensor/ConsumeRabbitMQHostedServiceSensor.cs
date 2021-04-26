
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
        private static string serialNumberInput = "";
        private static string deviceName = "";
        private static string valueInput = "";
        private static string serialNumberOutput = "";
        private static string ValueOutput = "";
        private static string MessageSend = "";
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
            //And splite message to Query Parameters (NP: Income Data message must same with data structure)
            string[] dataParsing = content.Split('#');
            foreach (var datas in dataParsing)
            {
                //System.Console.WriteLine($"{datas}>");
                serialNumberInput = dataParsing[0];
                deviceName = dataParsing[1];
                valueInput = dataParsing[2];
            }
            DateTime now = DateTime.Now;
            String timeStamp = now.ToString();
            // _logger.LogInformation($"consumer received guid {serialNumber}");
            // _logger.LogInformation($"consumer received device name {deviceName}");
            // _logger.LogInformation($"consumer received value {valueInput}");
            // _logger.LogInformation($"consumer received time {timeStamp}");
            //Open Connection Database
             connectionDB.Open();
            // _logger.LogInformation($"connection DB Opened");
            //Create command/query with param from mesage content  
            using (var transaction = connectionDB.BeginTransaction())
            {
            var insertCmd = connectionDB.CreateCommand();
            //     // _logger.LogInformation($"Try insert data to DB ...");
            insertCmd.CommandText = "insert INTO logsecurity (input_guid,name_device,value_device,time_stamp)VALUES(@SerialnumberInput,@device_name,@value_device,@time_stamp)";
            insertCmd.Parameters.AddWithValue("@SerialnumberInput", serialNumberInput);
            insertCmd.Parameters.AddWithValue("@device_name", deviceName);
            insertCmd.Parameters.AddWithValue("@value_device", valueInput);
            insertCmd.Parameters.AddWithValue("@time_stamp", timeStamp);
            //     //  _logger.LogInformation($"ESuccess Get Data from payload..");
            insertCmd.ExecuteNonQuery();
            //     //  _logger.LogInformation($"Execute Command Insert..");
            transaction.Commit();
            // }
            _logger.LogInformation($"success insert dats to DB");
            // connectionDB.Close();
            }
            if(deviceName == "BL_SMOKE" && valueInput == "1")
            {
                //connectionDB.Open();
                var selectCmd = connectionDB.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM rulesecurity  WHERE input_guid=@Guidinput AND name_device_input=@Valueinput";
                selectCmd.Parameters.AddWithValue("@Guidinput", serialNumberInput);
                selectCmd.Parameters.AddWithValue("@Valueinput", deviceName);
                using (var reader = selectCmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        serialNumberOutput = reader.GetString(3);
                        ValueOutput = reader.GetString(5);

                        _logger.LogInformation($"guid output{serialNumberOutput}");
                        _logger.LogInformation($"value output{ValueOutput}");
                         MessageSend = serialNumberOutput + "#" + ValueOutput;

                        _channel.BasicPublish(exchange: RMQExc,
                                            routingKey: RMQPubRoutingKey,
                                            basicProperties: null,
                                            body: Encoding.UTF8.GetBytes(MessageSend)
                    );

                    }

                    
                }
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
