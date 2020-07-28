
using System.Diagnostics;
using System.Text;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json.Linq;

namespace RegistrationServices
{
    class Program
    {

        /*
        Device variable
        */
        public static string serialNumber = "";
        public static string serialNumberDB = "";
        public static string macAddress = "";
        public static string typeDevice = "";
        public static string qtyDevice = "";
        public static string nameDevice = "";
        public static string versionDevice = "";
        public static string minorDevice = "";

        /////////////////////////////////////////////////////////

        /*
        MQTT Variable
        */

        public static string hostname = "192.168.4.2";
        public static int port = 5672;
        public static string user = "homeauto";
        public static string pass = "homeauto12345!";
        public static string vhost = "/Homeauto";
        public static string exchange = "amq.topic";
        public static string queue = "Reg";
        public static string routingKeySubscribe = "HomeautoIn";
        public static string routingKeyPublish = "Status";

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
        public static string statusRegister = "0";






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
                    serialNumber = dataJson.serialnumber;
                    macAddress = dataJson.mac;
                    typeDevice = dataJson.type;
                    qtyDevice = dataJson.qty;
                    nameDevice = dataJson.name;
                    versionDevice = dataJson.version;
                    minorDevice = dataJson.minor;
                    Console.WriteLine(serialNumber);
                    Console.WriteLine(macAddress);
                    Console.WriteLine(typeDevice);
                    Console.WriteLine(qtyDevice);
                    Console.WriteLine(nameDevice);
                    Console.WriteLine(versionDevice);
                    Console.WriteLine(minorDevice);

                    connectionDB.Open();
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
                            //outputGuid = reader.GetString(3);
                            //valueOutput = reader.GetString(4);

                            var dbSerialNumber = reader.GetString(1);
                            Console.WriteLine("ada data");

                            messageSend = serialNumber + "#" + statusRegistered;

                            channel.BasicPublish(
                            exchange: exchange,
                            routingKey: routingKeyPublish,
                            basicProperties: null,
                             body: Encoding.UTF8.GetBytes(messageSend)

                            );
                            Console.WriteLine("status device " + messageSend + " sent");
                        }
                        reader.Close();
                    }

                    else
                    {
                        Console.WriteLine("test");
                        using (var transaction = connectionDB.BeginTransaction())
                        {
                            Console.WriteLine("tidak ada data");
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
                            Console.WriteLine("Success Insert new data");

                            messageSend = serialNumber + "#" + statusRegister;

                            channel.BasicPublish(
                            exchange: exchange,
                            routingKey: routingKeyPublish,
                            basicProperties: null,
                             body: Encoding.UTF8.GetBytes(messageSend)

                            );
                            Console.WriteLine("status device " + messageSend + " sent");
                        }
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
