using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Device.Gpio;

namespace gpio_indicators
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string statusInternetIndicator;
        private string statusServerIndiator;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var path = @"/home/nurman/Documents/GitHub/service_homeauto_lskk/Worker/indicator.txt";

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs, Encoding.UTF8);

                string contents = sr.ReadToEnd();

                if (contents.Length > 0)
                {
                    string[] lines = contents.Split(new char[] { '\n' });
                    Dictionary<string, string> mysettings = new Dictionary<string, string>();
                    foreach (string line in lines)
                    {
                        string[] keyAndValue = line.Split(new char[] { '=' });
                        mysettings.Add(keyAndValue[0].Trim(), keyAndValue[1].Trim());
                    }

                    statusInternetIndicator = mysettings["internet"];
                    statusServerIndiator = mysettings["server"];


                }

                var pinInternerIndicator = 17;
                var pinServerIndicator = 22;

                using GpioController controller = new GpioController();
                controller.OpenPin(pinInternerIndicator,PinMode.Output);
                controller.OpenPin(pinServerIndicator,PinMode.Output);
                if (statusInternetIndicator == "1")
                {
                    controller.Write(pinInternerIndicator,PinValue.High);
                }
                if (statusInternetIndicator == "0")
                {
                    controller.Write(pinInternerIndicator,PinValue.Low);
                }
                if (statusServerIndiator == "1")
                {
                    controller.Write(pinServerIndicator,PinValue.High);
                }
                if (statusServerIndiator == "0")
                {
                    controller.Write(pinServerIndicator,PinValue.Low);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}
