using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Data.Sqlite;


namespace RmqServicesPptik
{
    class Program
    {
         public static string messageSend = "";
        public static string hostname = "167.205.7.226";
        public static int port = 5672;
        public static string user = "homeauto";
        public static string pass = "homeauto12345!";
        public static string vhost = "/homeauto";
        public static string exchange = "amq.topic";
        public static string queue = "Log";
        public static string routingKeySubscribe = "HomeautoIn";
        public static string routingKeyPublish = "Aktuator";

        public static string pathDatabase = "/home/nurman/Documents/Code/Worker/homeauto.db";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
