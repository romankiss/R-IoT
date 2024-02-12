using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.AtomLite;
using System.IO;
using System.Drawing;
using System.Text;
using Iot.Device.Ahtxx;


namespace websajt
{
    internal class Sensor
    {
        static double _temp = 0;
        static double _hum = 0;
        static string _telemetrydataFilePath = null;
        static Aht20 _sensor_temperature = null;
        public Sensor(Aht20 sensor_temperature, string telemetrydataFilePath)
        {
            _telemetrydataFilePath = telemetrydataFilePath;
            _sensor_temperature = sensor_temperature;

        }


        #region sensor
        public static void ReadData()
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Green);
            AtomLite.NeoPixel.Update();
            string body = File.ReadAllText(_telemetrydataFilePath);
            Debug.WriteLine($"{_telemetrydataFilePath}:\r\n{body}");
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }

        public static void DeleteData()
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Red);
            AtomLite.NeoPixel.Update();
            Debug.WriteLine("Deleting storage");
            if (File.Exists(_telemetrydataFilePath))
                File.Delete(_telemetrydataFilePath);
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }

        public static void WriteData(object state)
        {
            AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Blue);
            AtomLite.NeoPixel.Update();
            _temp = _sensor_temperature.GetTemperature().DegreesCelsius;
            _hum = _sensor_temperature.GetHumidity().Percent;

            var data = $"[{DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss")}] temp={_temp:F2}, hum={_hum:F2}\r\n";
            using (FileStream fs = new FileStream(_telemetrydataFilePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Debug.Write($"FileStorage.Write: {data}");
            }
            Thread.Sleep(100);
            AtomLite.NeoPixel.Image.Clear();
            AtomLite.NeoPixel.Update();
        }
        #endregion




    }
}