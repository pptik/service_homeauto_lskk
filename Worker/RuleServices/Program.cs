
using System.Diagnostics;
using System.Text;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json.Linq;

namespace RuleServices
{
    class Program
    {

        /*
        Device variable
        */
        public static string serialNumberOutput = "";
        public static string valueOutput = "";
        public static string serialNumberInput = "";
        public static string valueInput = "";


        /////////////////////////////////////////////////////////

        /*
        MQTT Variable
        */

        public static string hostname = "192.168.0.5";
        public static int port = 5672;
        public static string user = "homeauto";
        public static string pass = "homeauto12345!";
        public static string vhost = "/Homeauto";
        public static string exchange = "amq.topic";
        public static string queue = "Reg_rule";
        public static string routingKeySubscribe = "HomeautoIn";
        public static string routingKeyPublish = "Statusrule";

        /////////////////////////////////////////////////////////

        /*
        Path Physical Database file
        */
        public static string pathDatabase = "/home/nurman/Documents/GitHub/service_homeauto_lskk/Worker/homeauto.db";
        /////////////////////////////////////////////////////////



        /*
        Variable data and temporary
        */
        public static string messageSend = "";

        public static string statusRegistered = "1";
        public static string statusRegister = "Success Add Rule";






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
                    dynamic dataJson = JObject.Parse(message);
                    serialNumberInput = dataJson.guidinput;
                    valueInput = dataJson.valueinput;
                    serialNumberOutput = dataJson.guidoutput;
                    valueOutput = dataJson.valueoutput;
                    Console.WriteLine(serialNumberOutput);
                    Console.WriteLine(valueOutput);
                    Console.WriteLine(serialNumberInput);
                    Console.WriteLine(valueInput);

                    connectionDB.Open();

                    using (var transaction = connectionDB.BeginTransaction())
                    {
                        var insertCmd = connectionDB.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO activityiot (input_guid,input_value,output_guid,output_value) Values(@inputguid,@valueinput,@outputguid,@valueoutput)";
                        insertCmd.Parameters.AddWithValue("@inputguid", serialNumberOutput);
                        insertCmd.Parameters.AddWithValue("@valueinput", valueOutput);
                        insertCmd.Parameters.AddWithValue("@outputguid", serialNumberInput);
                        insertCmd.Parameters.AddWithValue("@valueoutput", valueInput);
                        insertCmd.ExecuteNonQuery();
                        transaction.Commit();
                        Console.WriteLine("Success Insert new data");

                        messageSend = statusRegister;

                        channel.BasicPublish(
                        exchange: exchange,
                        routingKey: routingKeyPublish,
                        basicProperties: null,
                         body: Encoding.UTF8.GetBytes(messageSend)

                        );
                        Console.WriteLine("status device " + messageSend + " sent");
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
