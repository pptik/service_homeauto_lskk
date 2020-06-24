using System.Diagnostics;
using System.Text;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json.Linq;
    
namespace Addrules
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
        public static string messageSend = "";
        public static string hostname = "192.168.0.5";
        public static int port = 5672;
        public static string user = "homeauto";
        public static string pass = "homeauto12345!";
        public static string vhost = "/Homeauto";
        public static string exchange = "amq.topic";
        public static string queue = "Reg";
        public static string routingKeySubscribe = "HomeautoIn";
        public static string routingKeyPublish = "Aktuator";
        /////////////////////////////////////////////////////////

        
        /*
        Path Physical Database file
        */
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
                    dynamic dataJson = JObject.Parse(message);
                        //System.Console.WriteLine($"{datas}>");


                    /*
                    Serialize Json data from mqtt
                    */
                    serialNumber    = dataJson.serialnumber;
                    macAddress      = dataJson.mac;
                    typeDevice      = dataJson.type;
                    qtyDevice       = dataJson.qty;
                    nameDevice      = dataJson.name;
                    versionDevice   = dataJson.version;
                    minorDevice     = dataJson.minor;
                    Console.WriteLine(serialNumber);
                    Console.WriteLine(macAddress);
                    Console.WriteLine(typeDevice);
                    Console.WriteLine(qtyDevice);
                    Console.WriteLine(nameDevice);
                    Console.WriteLine(versionDevice);
                    Console.WriteLine(minorDevice);
                    //////////////////////////////////////////////////////////////////////
                    //Console.WriteLine(macAddress);
                    connectionDB.Open();


                    /*
                    Select data command
                    */
                    var selectCmd  = connectionDB.CreateCommand();
                    selectCmd.CommandText = "SELECT * FROM registeriot  WHERE serial_number=@serial_number AND mac=@mac ";
                    selectCmd.Parameters.AddWithValue("@serialnumber",serialNumber);
                    selectCmd.Parameters.AddWithValue("@mac",macAddress);
                    ///////////////////////////////////////////////////////////////////////


                    /*
                    Insert data commmand
                    */
                    insertCmd.CommandText = "INSERT INTO registeriot (serial_number,mac,type,qty,name,version,minor) Values(@serialnumber,@mac,@type,@qty,@name,@version,@minor)";
                    insertCmd.Parameters.AddWithValue("@serialnumber", serialNumber);
                    insertCmd.Parameters.AddWithValue("@mac", macAddress);
                    insertCmd.Parameters.AddWithValue("@type", typeDevice);
                    insertCmd.Parameters.AddWithValue("@qty", qtyDevice);
                    insertCmd.Parameters.AddWithValue("@name",nameDevice);
                    insertCmd.Parameters.AddWithValue("@version", versionDevice);
                    insertCmd.Parameters.AddWithValue("@minor", minorDevice);



                    var readerSelect = selectCmd.ExecuteReader();

                    
                        if (readerSelect.HasRows)
                        {
                            while (readerSelect.Read())
                            {
                            
                                serialNumberDB = readerSelect.GetString(1);
                                Console.WriteLine("Data found : " ,serialNumberDB," ,Cant regist this Device");

                            }
                        }

                        else{


 

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
        




        public static void databaseInsert()
        {

            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "/home/nurman/Documents/GitHub/Service_Homeauto/Worker/homeauto.db";
            using (var connectionDB = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {


                connectionDB.Open();

                // var delTableCmd = connectionDB.CreateCommand();
                // delTableCmd.CommandText = "DROP TABLE IF EXISTS Rules";
                // delTableCmd.ExecuteNonQuery();

                // var createTableCmd = connectionDB.CreateCommand();
                // createTableCmd.CommandText = "CREATE TABLE Rules(Rulesid INTEGER PRIMARY KEY AUTOINCREMENT,Guidsensor VARCHAR(255) NOT NULL,Valuesensor VARCHAR(255) NOT NULL,Guidaktuator VARCHAR(255) NOT NULL,Valueaktuator VARCHAR(255) NOT NULL)";
                // createTableCmd.ExecuteNonQuery();

                using (var transaction = connectionDB.BeginTransaction())
                {
                    var insertCmd = connectionDB.CreateCommand();

                    Console.WriteLine("Enter Serial Number:");
                    serialNumber = Console.ReadLine();
                    Console.WriteLine("Your Mac Is: " + serialNumber);

                    Console.WriteLine("Enter Mac Device");
                    macAddress = Console.ReadLine();
                    Console.WriteLine("Your mac Is: " + macAddress);

                    Console.WriteLine("Enter Type Device:");
                    typeDevice = Console.ReadLine();
                    Console.WriteLine("Your Type Device Is: " + typeDevice);

                    
                    Console.WriteLine("Enter Quantity Pin Device:");
                    qtyDevice = Console.ReadLine();
                    Console.WriteLine("Your Mac Is: " + qtyDevice);

                    Console.WriteLine("Enter Name Device");
                    nameDevice = Console.ReadLine();
                    Console.WriteLine("Your Name Device Is: " + nameDevice);

                    Console.WriteLine("Enter Version Device:");
                    versionDevice = Console.ReadLine();
                    Console.WriteLine("Your Version Device Is: " + versionDevice);

                    Console.WriteLine("Enter minor Device:");
                    minorDevice = Console.ReadLine();
                    Console.WriteLine("Your Minor Device Is: " + minorDevice);





                    insertCmd.CommandText = "INSERT INTO registeriot (serial_number,mac,type,qty,name,version,minor) Values(@serialnumber,@mac,@type,@qty,@name,@version,@minor)";
                    insertCmd.Parameters.AddWithValue("@serialnumber", serialNumber);
                    insertCmd.Parameters.AddWithValue("@mac", macAddress);
                    insertCmd.Parameters.AddWithValue("@type", typeDevice);
                    insertCmd.Parameters.AddWithValue("@qty", qtyDevice);
                    insertCmd.Parameters.AddWithValue("@name",nameDevice);
                    insertCmd.Parameters.AddWithValue("@version", versionDevice);
                    insertCmd.Parameters.AddWithValue("@minor", minorDevice);


                    insertCmd.ExecuteNonQuery();

                    transaction.Commit();
                    Console.WriteLine("Success Update Data to Database");
                   



                }
                


            }
        }
    }
}
