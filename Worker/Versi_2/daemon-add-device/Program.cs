/***************************************************************************************************************************************
 *                                      DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                       *
 *                                                       VERSION             : 2                                                       *
 *                                                    TYPE APPLICATION    : WORKER                                                     *
 * DESCRIPTION         : GET DATA FROM MQTT (DEVICE) CHECK TO DB DEVICE AND SEND BACK (STATUS 0 = SUCESS SAVE DATA AND 1=IF DATA EXIST *
 ***************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace daemon_add_device
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsumeRabbitMQHostedService>();
                });
    }
}
