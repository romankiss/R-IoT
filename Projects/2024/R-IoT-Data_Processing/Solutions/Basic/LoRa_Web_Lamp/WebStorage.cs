// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using nanoFramework.System.IO.FileSystem;

namespace LoRa_Web_Lamp
{
   
    internal class WebStorage
    {
        static string _telemetrydataFilePath = null;
        public static string dat2 = null;

        static SDCard _sdcard = null;
        public WebStorage(string telemetrydataFilePath, SDCard sdcard)
        {
            _telemetrydataFilePath = telemetrydataFilePath;
            _sdcard = sdcard;
            MountMyCard();
        }

        public  void DeleteData()
        {
            Debug.WriteLine("Deleting storage");
            if (File.Exists(_telemetrydataFilePath))
                File.Delete(_telemetrydataFilePath);

        }

        public void ReadData()
        {
            string body = File.ReadAllText(_telemetrydataFilePath);
            Debug.WriteLine($"{_telemetrydataFilePath}:\r\n{body}");
        }

        public void WriteData(Lamp txt)
        {
            var data = JsonConvert.SerializeObject(txt) + ",\n";
            using (FileStream fs = new FileStream(_telemetrydataFilePath +""+ txt.SD.R+".json", FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Console.Write($"FileStorage.Write: {data}");
                long size = fs.Length;
                Console.WriteLine($"{size}");
            }
            string text = File.ReadAllText(_telemetrydataFilePath + "" + txt.SD.R + ".json");
            Debug.Write(text);
            string[] sub = text.Split('\n');
            Debug.WriteLine($"{sub.Length}");
            if (sub.Length > 20)
            {
                data = text.Substring((int)text.IndexOf('\n') + 1, text.Length - (int)text.IndexOf('\n') - 1);
                File.Delete(_telemetrydataFilePath + "" + txt.SD.R + ".json");
                using (FileStream fs = new FileStream(_telemetrydataFilePath + "" + txt.SD.R + ".json", FileMode.Append))
                {
                    fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                    Console.Write($"FileStorage.Write: {data}");
                    long size = fs.Length;
                    Console.WriteLine($"{size}");
                }
            }

        }

        public static void RefreshRvo(object state)
        {
            if (File.Exists("D:\\MainRvoData.json"))
                File.Delete("D:\\MainRvoData.json");
            for (int i = 0; i < 10; i++)
            {
                if (File.Exists(_telemetrydataFilePath+""+i+".json"))
                {
                    var data = File.ReadAllText(_telemetrydataFilePath+""+i+".json");
                    string[] sub = data.ToString().Split('\n');
                    using (FileStream fs = new FileStream("D:\\MainRvoData.json", FileMode.Append))
                    {
                        fs.Write(Encoding.UTF8.GetBytes(sub[sub.Length-2]), 0, sub[sub.Length - 2].Length);
                        Console.Write($"FileStorage.Write: {sub[sub.Length - 2]}");
                        long size = fs.Length;
                        Console.WriteLine($"{size}");

                    }
                }
            }
        }

        static bool MountMyCard()
        {
            try
            {
                _sdcard.Mount();
                Debug.WriteLine("Card Mounted");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Card failed to mount : {ex.Message}");
                Debug.WriteLine($"IsMounted {_sdcard.IsMounted}");
            }

            return false;
        }
    }
}

