using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Data.Sqlite;




namespace RmqServices
{
    class Program
    {
        public static string inputGuid = "";
        public static string valueInput = "";
        public static string outputGuid = "";
        public static string valueOutput = "";
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

                    connectionDB.Open();
                    //var transaction = connectionDB.BeginTransaction();
                    // var insertCmd = connectionDB.CreateCommand();
                    // insertCmd.CommandText = "INSERT INTO Rules (Guidsensor,Valuesensor,Guidaktuator,Valueaktuator) Values(@Guidsensor,@Valuesensor,@Guidaktuator,@Valueaktuator)";
                    // insertCmd.Parameters.AddWithValue("@Guidsensor", inputGuid);
                    // insertCmd.Parameters.AddWithValue("@Valuesensor", valueInput);
                    // insertCmd.Parameters.AddWithValue("@Guidaktuator", outputGuid);
                    // insertCmd.Parameters.AddWithValue("@Valueaktuator", valueOutput);


                    // insertCmd.ExecuteNonQuery();


                    var selectCmd = connectionDB.CreateCommand();
                    selectCmd.CommandText = "SELECT * FROM activityiot  WHERE input_guid=@Guidinput AND input_value=@Valueinput ";

                    selectCmd.Parameters.AddWithValue("@Guidinput", inputGuid);
                    selectCmd.Parameters.AddWithValue("@Valueinput", valueInput);

                    using (var reader = selectCmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            outputGuid = reader.GetString(3);
                            valueOutput = reader.GetString(4);


                            messageSend = outputGuid + "#" + valueOutput;

                            channel.BasicPublish(
                            exchange: exchange,
                            routingKey: routingKeyPublish,
                            basicProperties: null,
                            body: Encoding.UTF8.GetBytes(messageSend)

                        );
                        Console.WriteLine(messageSend);
                        }




                        connectionDB.Close();
                    }



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
