//Nevyskúšaný na HW, robený pre ESP32_S3 a BMP280 senzor v ENV_IV kombajne pripojený cez Grove konektor

using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Bmxx80;

namespace TESTER
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");



            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            Configuration.SetPinFunction(2, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(1, DeviceFunction.I2C1_CLOCK);

            // bus id on the MCU
            const int busId = 1;

            I2cConnectionSettings i2cSettings = new(busId, Bmp280.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            using var i2CBmp280 = new Bmp280(i2cDevice);

            // set higher sampling
            i2CBmp280.TemperatureSampling = Sampling.LowPower;
            i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

            // Perform a synchronous measurement
            var readResult = i2CBmp280.Read();

            // Print out the measured data
            Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius:0.#}\u00B0C");
            Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals:0.##}hPa");
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
