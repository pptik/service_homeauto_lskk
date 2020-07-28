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
        public static string guidOutput= "";
        public static string valueOutput= "";
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

        public static string pathDatabase = "/home/nurman/Documents/Code/Worker/logs.db";

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
                        guidOutput = dataParsing[0];
                        valueOutput = dataParsing[1];

                    }

                    using (var transaction = connectionDB.BeginTransaction())
                    {
                        var insertCmd = connectionDB.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO logs (output_guid_device,output_value_device,time_device) Values(@guid,@value,@time)";
                        insertCmd.Parameters.AddWithValue("@guid", guidOutput);
                        insertCmd.Parameters.AddWithValue("@value",valueOutput);
                        insertCmd.Parameters.AddWithValue("@time", now);
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

