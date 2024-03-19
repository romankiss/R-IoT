using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace AccelDisplay
{
    internal class StorageHandler
    {
        public static string _filePath;

        public StorageHandler(string filePath)
        {
            _filePath = filePath;
        }

        public void DeleteData()
        {
            Debug.WriteLine("Deleting storage");
            if (File.Exists(_filePath))
                File.Delete(_filePath);
            Thread.Sleep(100);

        }

        public void WriteData(string data)
        {
            using (FileStream fs = new FileStream(_filePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            }
            Thread.Sleep(100);

        }

        public void ReadData()
        {
            string body = File.ReadAllText(_filePath);
            Debug.WriteLine($"{_filePath}:\r\n{body}");
            Thread.Sleep(100);

        }

    }
}
