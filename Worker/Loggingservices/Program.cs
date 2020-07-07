using System;
using System.Text;
using System.Globalization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Data.Sqlite;

namespace Loggingservices
{
    class Program
    {
        /**
          * Get time from device
          * */
        public static DateTime now = DateTime.Now;

        /**
         * RMQ and message variable  
         **/
        public static string inputGuid = "";
        public static string valueInput = "";
        public static string messageSend = "";
        public static string hostname = "192.168.0.5";
        public static int port = 5672;
        public static string user = "homeauto";
        public static string pass = "homeauto12345!";
        public static string vhost = "/Homeauto";
        public static string exchange = "amq.topic";
        public static string queue = "Sensor";
        public static string routingKeySubscribe = "HomeautoIn";
        public static string routingKeyPublish = "Aktuator";

        public static string pathDatabase = "/home/nurman/Documents/Code/Worker/homeauto.db";

        static void Main(string[] args)
        {


            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = pathDatabase;
            var connectionDB = new SqliteConnection(connectionStringBuilder.ConnectionString);


            var factory = new ConnectionFactory() { HostName = hostname, Port = port, UserName = user, Password = pass, VirtualHost = vhost };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    //Console.Write("[x] Received {0}",message);
                    string[] dataParsing = message.Split('#');
                    foreach (var datas in dataParsing)
                    {
                        //System.Console.WriteLine($"{datas}>");
                        inputGuid = dataParsing[0];
                        valueInput = dataParsing[1];

                    }

                    using (var transaction = connectionDB.BeginTransaction())
                    {
                        var insertCmd = connectionDB.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO logsiot (output_guid,output_value,time_device) Values(@guid,@value,@time)";
                        insertCmd.Parameters.AddWithValue("@guid", inputGuid);
                        insertCmd.Parameters.AddWithValue("@value", v);
                        insertCmd.Parameters.AddWithValue("@time", serialNumberInput);
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                        Console.WriteLine("Success Insert new data");
                    }
                    connectionDB.Close();

                };
                channel.BasicConsume(queue: queue,
                                     autoAck: true,
                                     consumer: consumer
                                    );
                Console.WriteLine("Press Any Key to Close");
                Console.ReadLine();
            }

        }



    }
}

