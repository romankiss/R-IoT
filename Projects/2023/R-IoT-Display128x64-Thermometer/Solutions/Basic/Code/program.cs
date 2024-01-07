using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework;
using Iot.Device.Ssd13xx;
using System.Drawing;
using Iot.Device.Ws28xx.Esp32;
using System.Device.Gpio;
using Iot.Device.Button;
using Iot.Device.Sht3x;
using nanoFramework.AtomLite;
using Iot.Device.Ahtxx;
using UnitsNet;
using System.IO.Ports;

namespace Display
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int i = 0;
        static Aht20 aht20 = null;

        public static void Main()
        {
            GpioController gpio = new GpioController();
            neo = new Sk6812(27, 3);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)

            //display pins
            #region Display 128x64
            Configuration.SetPinFunction(21, DeviceFunction.I2C2_CLOCK);
            Configuration.SetPinFunction(25, DeviceFunction.I2C2_DATA);

            //thermometer pins
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C1_CLOCK);
            
            // Create an instance of I2cDevice using the appropriate settings for your device
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Aht20.DefaultI2cAddress); // Replace with the correct I2C bus index and address
            I2cDevice i2cDevice = I2cDevice.Create(settings);
            
            Aht20 aht20Sensor = new Aht20(i2cDevice);

            //AHT20 thermometer
            #region Temp&Hum  - sensor aht20   
            var i2c_aht20 = AtomLite.GetGrove(Aht20.DefaultI2cAddress);
            var res = i2c_aht20.WriteByte(0x07);
            //AH
            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                aht20 = new Aht20(i2c_aht20);  //seeed, aht20+BMP280,     = 0x38
                Debug.WriteLine($"Temp = {aht20.GetTemperature().DegreesCelsius} °C, Hum = {aht20.GetHumidity().Percent} %");
            }
            #endregion
            //====================================================//


            //button
            GpioButton button = new GpioButton(buttonPin: 39);
            button.Press += (sender, e) =>
            {
                Debug.WriteLine($"Press");

            //display font, display
            I2cDevice i2c_oled = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled);
            display.ClearScreen();
            display.Font = new Sinclair8x8();

            //temperature and humidity
            double temperature = aht20.GetTemperature().DegreesCelsius;
            double humidity = aht20.GetHumidity().Percent;
            
            //getting time
            DateTime currentTime = DateTime.UtcNow; // Get current UTC time
            string timeString = currentTime.ToString("HH:mm:ss"); // Format time as HH:mm:ss

            DateTime utcNow = DateTime.UtcNow;
            Debug.WriteLine("Current UTC time is: " + utcNow);

            //-----------------------------------//

            //deploying things on display
            display.ClearScreen();

                display.DrawHorizontalLine(0, 0, 127);
                display.DrawHorizontalLine(0, 63, 127);
                display.DrawVerticalLine(0, 0, 63);
                display.DrawVerticalLine(127, 0, 63);

                
                byte[] bitmap = { 0b111, 0b101, 0b111 };//°
                display.DrawBitmap(98, 21, 1, 3, bitmap);//°

                display.DrawBitmap(102,20, 1, 8, display.Font['C']);


            //all important values
            display.DrawString(5, 20, $"Temp: {temperature:F2}");//°
            display.DrawString(5, 30, $"Humi: {humidity:F2} %");
            display.DrawString(5, 4, $"Time: {timeString}");//time
            display.DrawHorizontalLine(0, 15, 127);

            display.Display();

            Debug.WriteLine("----------");

            #endregion
            
            };
            Thread.Sleep(Timeout.Infinite);
        }

    }
}


