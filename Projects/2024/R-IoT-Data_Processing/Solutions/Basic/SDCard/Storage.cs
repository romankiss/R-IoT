using System;
using nanoFramework.System.IO.FileSystem;
using nanoFramework.System.IO;
using System.Text;
using System.Diagnostics;
using System.IO;
using nanoFramework.Json;


namespace SD_card
{
    internal class WebStorage
    {
        static string _telemetrydataFilePath = null;
        static SDCard _sdcard = null;
        
        public WebStorage(string telemetrydataFilePath,SDCard sdcard)
        {
            _telemetrydataFilePath = telemetrydataFilePath;
            _sdcard = sdcard;
            MountMyCard();
        }


        public void DeleteData()
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

        public void WriteData(string txt)
        {
            
            
                var data = txt + ",\r";
                using (FileStream fs = new FileStream(_telemetrydataFilePath, FileMode.Append))
                {
                    fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                    Console.Write($"FileStorage.Write: {data}");
                    long size = fs.Length;
                    Console.WriteLine($"{size}");

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
