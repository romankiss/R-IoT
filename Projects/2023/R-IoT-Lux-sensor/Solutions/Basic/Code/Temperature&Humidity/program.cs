using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Ahtxx;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;

namespace temperature
{
    public class Program
    {

        static Aht20 sensor_temperature = null;
        
        public static void Main()
        {

            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));

            Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);

            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Aht20.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            sensor_temperature = new Aht20(i2cDevice);


            button.Press += (sender, e) => Temp();


            

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
        private static void Temp()
        {
            if (sensor_temperature != null)
            {
            Debug.WriteLine($"sensorTH: temperature[C]={sensor_temperature.GetTemperature().DegreesCelsius:F2}, humidity[%]={sensor_temperature.GetHumidity().Percent:F2}");

            }
        }

    }
}
