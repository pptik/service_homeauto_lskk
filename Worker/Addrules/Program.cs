using System.Diagnostics;
using System.Text;
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Addrules
{
    class Program
    {
        public static string btnName = "";
        public static string vlButton = "";
        public static string lmpName = "";
        public static string vLamp = "";
        static void Main(string[] args)
        {
            databaseInsert();




        }




        public static void databaseInsert()
        {

            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "/home/nurman/Documents/Code/Worker/Rulesiot.db";
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

                    Console.WriteLine("Enter Name Button:");
                    btnName = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + btnName);

                    Console.WriteLine("Enter Value Button:");
                    vlButton = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + vlButton);

                    Console.WriteLine("Enter Name Lamp:");
                    lmpName = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + lmpName);

                    Console.WriteLine("Enter Name Button:");
                    vLamp = Console.ReadLine();
                    Console.WriteLine("Your Button Is: " + vLamp);

                    insertCmd.CommandText = "INSERT INTO Rules (Guidsensor,Valuesensor,Guidaktuator,Valueaktuator) Values(@Guidsensor,@Valuesensor,@Guidaktuator,@Valueaktuator)";
                    insertCmd.Parameters.AddWithValue("@Guidsensor", btnName);
                    insertCmd.Parameters.AddWithValue("@Valuesensor", vlButton);
                    insertCmd.Parameters.AddWithValue("@Guidaktuator", lmpName);
                    insertCmd.Parameters.AddWithValue("@Valueaktuator", vLamp);
                    Console.WriteLine(btnName);
                    Console.WriteLine(vlButton);
                    Console.WriteLine(lmpName);
                    Console.WriteLine(vLamp);

                    insertCmd.ExecuteNonQuery();

                    transaction.Commit();
                    Console.WriteLine("Berhasil update data");




                }


            }
        }
    }
}
