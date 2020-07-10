using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Timers;
namespace FtpRulesDeviceDbServices
{
    class Program
    {
        public static string urlRequestFTP = "ftp://homeauto@ftp5.pptik.id:2121/homeauto/homeauto";
        public static string credentialUsername = "homeauto";
        public static string credentialPassword = "auTo2200|";
        public static string readPath = "/home/nurman/Documents/GitHub/service_homeauto_lskk/Worker/homeauto.db";
        public static Timer aTimer;
        public static void PostDatatoFTP(Object source, System.Timers.ElapsedEventArgs a)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(urlRequestFTP + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".db");
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential(@credentialUsername, credentialPassword);
                var fs = File.OpenRead(@readPath);
                Stream requestStream = request.GetRequestStream();
                fs.CopyTo(requestStream);
                requestStream.Close();
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription, a.SignalTime);

                response.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message.ToString());
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                Console.WriteLine(status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }


        static void Main(string[] args)
        {
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 43200000; //In Milliseconds

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += PostDatatoFTP;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();

            // Program.PostDatatoFTP();

        }

    }
}
