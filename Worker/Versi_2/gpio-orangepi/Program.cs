﻿using System;
using System.Device.Gpio;
using System.Threading;

namespace gpio_orangepi
{
 class Program
    {
        static void Main(string[] args)
        {
            var pin = 17;

            var lightTimeInMilliseconds = 1000;
            var dimTimeInMilliseconds = 200;
            
            Console.WriteLine($"Let's blink an LED!");
            using (GpioController controller = new GpioController())
            {
                controller.OpenPin(pin, PinMode.Output);
                Console.WriteLine($"GPIO pin enabled for use: {pin}");

                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                {
                    controller.Dispose();
                };

                while (true)
                {
                    Console.WriteLine($"Light for {lightTimeInMilliseconds}ms");
                    controller.Write(pin, PinValue.High);
                    Thread.Sleep(lightTimeInMilliseconds);
                    Console.WriteLine($"Dim for {dimTimeInMilliseconds}ms");
                    controller.Write(pin, PinValue.Low);
                    Thread.Sleep(dimTimeInMilliseconds);
                }
            }
        }
    }
}
