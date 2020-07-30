/***************************************************************************************************************************************
 *                                      DEVELOPMENT BY      : NURMAN HARIYANTO - PT.LSKK & PPTIK                                       *
 *                                                       VERSION             : 2                                                       *
 *                                                    TYPE APPLICATION    : WORKER                                                     *
 * DESCRIPTION         : GET DATA FROM MQTT (DEVICE) CHECK TO DB DEVICE AND SEND BACK (STATUS 0 = SUCESS SAVE DATA AND 1=IF DATA EXIST *
 ***************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace daemon_add_device
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
