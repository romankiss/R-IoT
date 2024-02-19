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
        public static int pocet = 0;
        public static string dat2 = null;
        public Sensor(Aht20 sensor_temperature, string telemetrydataFilePath)
        {
            _telemetrydataFilePath = telemetrydataFilePath;
            _sensor_temperature = sensor_temperature;

        }


        #region sensor
        public static void ReadData()
        {
            string body = File.ReadAllText(_telemetrydataFilePath);
            Debug.WriteLine($"{_telemetrydataFilePath}:\r\n{body}");
            websajt.Blink.Blinks(0, 255, 0);
        }

        public static void DeleteData()
        {
            Debug.WriteLine("Deleting storage");
            if (File.Exists(_telemetrydataFilePath))
                File.Delete(_telemetrydataFilePath);
            websajt.Blink.Blinks(255, 0, 0);

        }

        public static void WriteData(object state)
        {
            _temp = _sensor_temperature.GetTemperature().DegreesCelsius;
            _hum = _sensor_temperature.GetHumidity().Percent;

            var data = $"[{DateTime.UtcNow.ToString("dd:MM:yyyy hh:mm:ss")}] temp={_temp:F2}, hum={_hum:F2}\r\n";
            using (FileStream fs = new FileStream(_telemetrydataFilePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Debug.Write($"FileStorage.Write: {data}");
            }
            websajt.Blink.Blinks(0, 0, 255);
            if (pocet < 20)
            {
                pocet = 0;
                byte[] Zisti = File.ReadAllBytes(_telemetrydataFilePath);
                foreach (byte Z in Zisti)
                {
                    if (Z == 10)
                    {
                        ++pocet;
                    }
                }
                Debug.WriteLine($"{pocet}");
            }
            string dat = null;
            int i = 0;
            if (pocet > 19)
            {
                byte[] lines = File.ReadAllBytes(_telemetrydataFilePath);
                foreach (byte s in lines)
                {
                    if (i == 1)
                    {

                        char c = (char)s;

                        dat2 = c.ToString();
                        dat = dat + dat2;
                    }
                    if (s == 10)
                    {
                        i = 1;
                    }
                }
                byte[] datas = Encoding.UTF8.GetBytes(dat);
                File.WriteAllBytes(_telemetrydataFilePath, datas);
            }
        }
        #endregion




    }
}