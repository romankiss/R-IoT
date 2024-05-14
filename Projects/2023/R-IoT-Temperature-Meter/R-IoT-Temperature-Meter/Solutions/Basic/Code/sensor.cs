using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Iot.Device.Sht3x;
using nanoFramework.Hardware.Esp32;


namespace WebServerTest
{
    internal class Sensor
    {
        static double _temp = 0;
        static double _hum = 0;
        static int HowMany = 1;
        static string _telemetrydataFilePath = null;
        static Sht3x _sensor_temperature = null;
        public static int pocet = 0;
        public static string dat2 = null;
        public Sensor(Sht3x sensorTH, string telemetrydataFilePath)
        {
            _telemetrydataFilePath = telemetrydataFilePath;
            _sensor_temperature = sensorTH;

        }


        #region sensor
        public static void ReadData()
        {
            string body = File.ReadAllText(_telemetrydataFilePath);
            Debug.WriteLine($"{_telemetrydataFilePath}:\r\n{body}");

        }

        public static void DeleteData()
        {
            Debug.WriteLine("Deleting storage");
            if (File.Exists(_telemetrydataFilePath))
                File.Delete(_telemetrydataFilePath);


        }

        public static void WriteData(object state)
        {
            _temp = _sensor_temperature.Temperature.DegreesCelsius;
            _hum = _sensor_temperature.Humidity.Percent;

            var data = $"[{DateTime.UtcNow.ToString("dd:MM:yyyy hh:mm:ss")}] temp={_temp:F2}, hum={_hum:F2}\r\n";
            using (FileStream fs = new FileStream(_telemetrydataFilePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Debug.Write($"FileStorage.Write: {data}");
                long size = fs.Length;
                Debug.WriteLine($"{size}");
                if (size > 1000)
                {
                    string lines = File.ReadAllText(_telemetrydataFilePath);
                    int result = lines.IndexOf('\n');
                    string datas = lines.Substring(0, (result + 1) * HowMany);

                    Debug.WriteLine($"{result}");
                    File.WriteAllText(_telemetrydataFilePath, datas);
                }
            }

        }
        #endregion




    }
}
