using System;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Text;
using System.Globalization;


namespace testConnectionFTP
{
    
    class Program
    {
        public static DateTime now = DateTime.Now;
        public static void PostDatatoFTP(int i)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://YourHostname.com" + @"\" + "TestFile" + i.ToString() + ".txt");
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential("maruthi", "*******");
                // Copy the contents of the file to the request stream.
                StreamReader sourceStream = new StreamReader(@"E:\YourLoaction\SampleFile.txt");
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

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
    }
}
