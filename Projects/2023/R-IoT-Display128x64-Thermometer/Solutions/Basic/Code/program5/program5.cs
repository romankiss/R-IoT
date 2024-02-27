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
using System.Net;
using System.Net.Sockets;
using System.Device.Wifi;
using Iot.Device.Bh1750fvi;
using System.Text;
using System.IO;
using Wifii;

namespace Display
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static Aht20 aht20 = null;
        static Bh1750fvi bh1750fvi = null;
        static string telemetrydataFilePath = null;

        const string MYSSID = "AS PIESTANY 3";
        const string MYPASSWORD = "123456789";

        public static void Main()
        {
            GpioController gpio = new GpioController();

            #region PINS
            //hub 
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(25, DeviceFunction.I2C1_DATA);
            #endregion

            #region I2c
            //thermometer
            I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x38); // Replace with the correct I2C bus index and address
            I2cDevice i2cDevice = I2cDevice.Create(settings);
            aht20 = new Aht20(i2cDevice);

            //light sensore
            I2cConnectionSettings settings1 = new I2cConnectionSettings(1, 0x23);
            I2cDevice i2cdevice1 = I2cDevice.Create(settings1);
            bh1750fvi = new Bh1750fvi(i2cdevice1);
            #endregion

            //====================================================//
            
            #region Display
            I2cDevice i2cOled = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2cOled);
            display.ClearScreen();
            display.Font = new Sinclair8x8();

            //display overlay
            display.DrawHorizontalLine(0, 0, 127);
            display.DrawHorizontalLine(0, 63, 127);
            display.DrawVerticalLine(0, 0, 63);
            display.DrawVerticalLine(127, 0, 63);
            display.DrawHorizontalLine(0, 15, 127);
            display.Display();
            #endregion
          
            #region File
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;

            const string DirectoryPath = "I:\\";
            telemetrydataFilePath = DirectoryPath + "telemetryData.json";

            Timer pub_Timer = new Timer(WriteData, null, 10000, 30000);

            if (!File.Exists(telemetrydataFilePath))
            {
                AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Yellow);
                AtomLite.NeoPixel.Update();
                File.Create(telemetrydataFilePath);
                Thread.Sleep(100);
                AtomLite.NeoPixel.Image.Clear();
                AtomLite.NeoPixel.Update();
            }
            #endregion

            Wifi wifi = new Wifi(MYSSID, MYPASSWORD, telemetrydataFilePath);

            button.Press += (sender, e) =>
            {
                button.Holding += (sender, e) => DeleteData();
                string[] listFiles = Directory.GetFiles(DirectoryPath);
                Debug.WriteLine("FileStorage:");
                foreach (var file in listFiles)
                {
                    Debug.WriteLine($" {file}");
                }

                ReadData();

                Debug.WriteLine($"Press");

                UpdateDisplay(display);

                

            };

            Thread.Sleep(Timeout.Infinite);
        }

        //========================================//

        private static void UpdateDisplay(Ssd1306 display)
        {
            
            double temperature = aht20.GetTemperature().DegreesCelsius;
            double humidity = aht20.GetHumidity().Percent;
            double illuminance = bh1750fvi.Illuminance.Lux;

            DateTime currentTime = DateTime.UtcNow;
            DateTime time = currentTime.AddHours(1); //+1
            string timeString = time.ToString("HH:mm:ss");
            string dateString = time.ToString("dd/MM/yy");


            Debug.WriteLine("Time =  " + time); 
            Debug.WriteLine($"LUX = {illuminance}");
            Debug.WriteLine($"Temperature = {temperature}");
            Debug.WriteLine($"Humidity = {humidity}");

            byte[] bitmap = { 0b111,
                              0b101,
                              0b111 };//°
            display.DrawBitmap(98, 21, 1, 3, bitmap);//°

            display.DrawBitmap(102, 20, 1, 8, display.Font['C']);

            //all important values
            display.DrawString(5, 20, $"Temp: {temperature:F2}");
            display.DrawString(5, 30, $"Humi: {humidity:F2} %");
            display.DrawString(5, 40, $"LUX : {illuminance:F1}");
            display.DrawString(5, 4, $"Time: {timeString}");
            display.DrawString(5, 50, $"Date: {dateString}");
            
            display.Display();

            Debug.WriteLine("----------");
        }
       
        public static void ReadData()
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Green);
            AtomLite.NeoPixel.Update();
            string body = File.ReadAllText(telemetrydataFilePath);
            Debug.WriteLine($"{telemetrydataFilePath}:\r\n{body}");
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }

        public static void DeleteData()
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Red);
            AtomLite.NeoPixel.Update();
            Debug.WriteLine("Deleting storage");
            if (File.Exists(telemetrydataFilePath))
                File.Delete(telemetrydataFilePath);
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }

        public static void WriteData(object state)
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Blue);
            AtomLite.NeoPixel.Update();

            double temperature = aht20.GetTemperature().DegreesCelsius;
            double humidity = aht20.GetHumidity().Percent;
            double illuminance = bh1750fvi.Illuminance.Lux;

            var data = $"[{DateTime.UtcNow.ToString("hh:mm:ss")}] temp={temperature:F2}, hum={humidity:F2}, lux={illuminance:F2}\r\n <br>";
            using (FileStream fs = new FileStream(telemetrydataFilePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Debug.Write($"FileStorage.Write: {data}");
            }
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }

    }
}


