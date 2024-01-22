
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Bh1750fvi;
using Iot.Device.Ssd13xx;
using Iot.Device.Button;
using System;

namespace Projekt_26._12._2023
{
    public class Program
    {
        public static void Main()
        {
            double illuminance;
            GpioButton button = new GpioButton(buttonPin: 39 , debounceTime:TimeSpan.FromMilliseconds(200));

            Debug.WriteLine("Hello from nanoFramework!");
            Configuration.SetPinFunction(23, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(33, DeviceFunction.I2C1_CLOCK);

            Configuration.SetPinFunction(21, DeviceFunction.I2C2_CLOCK);
            Configuration.SetPinFunction(25, DeviceFunction.I2C2_DATA);

            I2cConnectionSettings settings = new I2cConnectionSettings(busId: 1, (int)I2cAddress.AddPinLow);
            I2cDevice device = I2cDevice.Create(settings);


            I2cDevice i2c_oled128x64 = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled128x64);
            var sensor = new Bh1750fvi(device);

            display.Font = new Sinclair8x8();
            display.ClearScreen();

            button.Press += (sender, e) =>
            {
                illuminance = sensor.Illuminance.Value; //read illuminance(Lux)
                Debug.WriteLine($"LUX = {illuminance}");
                display.Write(0, 5, $"LUX = {illuminance}");
                display.DrawHorizontalLine(0, 50, 127);
                display.Display();
            };



            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
        
    }
}
