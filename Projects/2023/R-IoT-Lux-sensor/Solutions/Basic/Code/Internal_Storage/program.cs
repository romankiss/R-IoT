using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Ahtxx;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Button;
using nanoFramework.AtomLite;
using System.Drawing;


namespace BasicFileSystemExample
{
    public class Program
    {
        //Zelená Vypísal data = button press
        //Červená Zmazanie Zložky z datami = button hold
        //Žltá Vytvorenie zložky = automaticky iba pri zapnutí
        //Modrá Zápis do zložky = automaticky za každých 10 min


        static Aht20 sensor_temperature = null;
        static double temp = 0;
        static double hum = 0;
        static string telemetrydataFilePath = null;

        public static void Main()
        {
            // E = USB storage
            // D = SD Card
            // I = Internal storage
            const string DirectoryPath = "I:\\";
            telemetrydataFilePath = DirectoryPath + "telemetryData.json";

            Timer pub_Timer = new Timer(WriteData, null, 10000 , 600000);

            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;

            Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);



            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Aht20.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            sensor_temperature = new Aht20(i2cDevice);

            button.Holding += (sender, e) => DeleteData();


            if (!File.Exists(telemetrydataFilePath))
            {
                AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Yellow);
                AtomLite.NeoPixel.Update();
                File.Create(telemetrydataFilePath);
                Thread.Sleep(100);
                AtomLite.NeoPixel.Image.Clear();
                AtomLite.NeoPixel.Update();
            }
            

            string[] listFiles = Directory.GetFiles(DirectoryPath);
            Debug.WriteLine("FileStorage:");
            foreach (var file in listFiles)
            {
                Debug.WriteLine($" {file}");
            }

            button.Press += (sender, e) => ReadData();

           

            Thread.Sleep(Timeout.Infinite);
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
            temp = sensor_temperature.GetTemperature().DegreesCelsius;
            hum = sensor_temperature.GetHumidity().Percent;

            var data = $"[{DateTime.UtcNow.ToString("hh:mm:ss")}] temp={temp:F2}, hum={hum:F2}\r\n";
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
