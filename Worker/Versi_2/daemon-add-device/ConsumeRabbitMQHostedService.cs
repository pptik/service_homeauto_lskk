/***************************************************************************************************************************************
 *                                      DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                       *
 *                                                       VERSION             : 2                                                       *
 *                                                    TYPE APPLICATION    : WORKER                                                     *
 * DESCRIPTION         : GET DATA FROM MQTT (DEVICE) CHECK TO DB DEVICE AND SEND BACK (STATUS 0 = SUCESS SAVE DATA AND 1=IF DATA EXIST *
 ***************************************************************************************************************************************/


namespace daemon_add_device
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
        private static string serialNumber = "";
        private static string macAddress = "";
        private static string typeDevice = "";
        private static string qtyDevice = "";
        private static string nameDevice = "";
        private static string versionDevice = "";
        private static string minorDevice = "";
        private static string statusRegistered = "1";
        private static string statusRegister = "0";


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
            _logger.LogInformation($"consumer received {content}");

            //And splite message to Query Parameters (NP: Income Data message must same with data structure)
            dynamic dataJson = JObject.Parse(content);
            serialNumber = dataJson.serialnumber;
            macAddress = dataJson.mac;
            typeDevice = dataJson.type;
            qtyDevice = dataJson.qty;
            nameDevice = dataJson.name;
            versionDevice = dataJson.version;
            minorDevice = dataJson.minor;

            //Open Connection Database
            connectionDB.Open();

            //Create command/query with param from mesage content  
            var selectCmd = connectionDB.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM registeriot  WHERE serial_number=@serial_number AND mac=@mac";
            selectCmd.Parameters.AddWithValue("@serial_number", serialNumber);
            selectCmd.Parameters.AddWithValue("@mac", macAddress);

            SqliteDataReader reader = null;
            reader = selectCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {

                    var dbSerialNumber = reader.GetString(1);

                    MessageSend = serialNumber + "#" + statusRegistered;

                    _channel.BasicPublish(
                    exchange: RMQExc,
                    routingKey: RMQPubRoutingKey,
                    basicProperties: null,
                     body: Encoding.UTF8.GetBytes(MessageSend)

                    );
                }
                reader.Close();
            }

            else
            {
                using (var transaction = connectionDB.BeginTransaction())
                {

                    var insertCmd = connectionDB.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO registeriot (serial_number,mac,type,qty,name,version,minor) Values(@serialnumber,@mac,@type,@qty,@name,@version,@minor)";
                    insertCmd.Parameters.AddWithValue("@serialnumber", serialNumber);
                    insertCmd.Parameters.AddWithValue("@mac", macAddress);
                    insertCmd.Parameters.AddWithValue("@type", typeDevice);
                    insertCmd.Parameters.AddWithValue("@qty", qtyDevice);
                    insertCmd.Parameters.AddWithValue("@name", nameDevice);
                    insertCmd.Parameters.AddWithValue("@version", versionDevice);
                    insertCmd.Parameters.AddWithValue("@minor", minorDevice);
                    insertCmd.ExecuteNonQuery();
                    transaction.Commit();


                    MessageSend = serialNumber + "#" + statusRegister;

                    _channel.BasicPublish(
                    exchange: RMQExc,
                    routingKey: RMQPubRoutingKey,
                    basicProperties: null,
                     body: Encoding.UTF8.GetBytes(MessageSend)

                    );
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
