using System;
using System.Device.Spi;
using System.Diagnostics;

using System.IO;
//using System.Collections.Generic;
using System.Text;

namespace websajt
{
    internal class Storage
    {

        public static void init(string file_path)
        {
            if (!File.Exists(file_path))
            {
                File.Create(file_path);
            }
        }
        public static string read(string file_path)
        {
            if (File.Exists(file_path))
            {
                string data = File.ReadAllText(file_path);
                return data;
            }
            else
            {
                return null;
            }

        }

        public static void append(string file_path, string data)
        {
            Storage.init(file_path);
            using (FileStream fs = new FileStream(file_path, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
                Debug.Write($"FileStorage.Write: {data}\n");
            }

        }
        public static bool delete(string file_path)
        {
            if (File.Exists(file_path))
            {
                File.Delete(file_path);
                return true;
            }
            else { return false; }
        }
        public static void clear(string file_path)
        {
            File.WriteAllText(file_path, "");
        }
        public static void list_files(string directory_path)
        {
            string[] listFiles = Directory.GetFiles(directory_path);
            Debug.WriteLine("FileStorage:");
            foreach (var file in listFiles)
            {
                Debug.WriteLine($" {file}");
            }
        }
        public static void new_formated_record(string file_path, string data)
        {
            var formated_data = $"[{DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss")}] sensor_measurement={data}\r\n";
            append(file_path, formated_data);
        }
    }
}
