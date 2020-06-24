using System.Diagnostics;
using System.Text;
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Addrules
{
    class Program
    {
        public static string macAddress = "";
        public static string guidDevice = "";
        public static string typeDevice = "";
        static void Main(string[] args)
        {
            databaseInsert();




        }




        public static void databaseInsert()
        {

            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "/home/nurman/Documents/Code/Worker/homeauto.db";
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

                    Console.WriteLine("Enter Mac Device:");
                    macAddress = Console.ReadLine();
                    Console.WriteLine("Your Mac Is: " + macAddress);

                    Console.WriteLine("Enter Guid Device");
                    guidDevice = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + guidDevice);

                    Console.WriteLine("Enter Type Device:");
                    typeDevice = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + typeDevice);



                    insertCmd.CommandText = "INSERT INTO registeriot (mac,guid,type) Values(@mac,@guid,@type)";
                    insertCmd.Parameters.AddWithValue("@mac", macAddress);
                    insertCmd.Parameters.AddWithValue("@guid", guidDevice);
                    insertCmd.Parameters.AddWithValue("@type", typeDevice);


                    insertCmd.ExecuteNonQuery();

                    transaction.Commit();
                    Console.WriteLine("Success Update Data to Database");




                }


            }
        }
    }
}
