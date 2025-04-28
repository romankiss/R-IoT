using System;
//using System.Device.Spi;
using System.Diagnostics;

using System.IO;
//using System.Collections.Generic;
using System.Text;

//BE AWARE WHEN INSTALLING THE STORAGE NUGGET - make sure it starts with nanoFramework in its name and has its logo, if you install the .NET Storage this wont work

namespace CanSat-28.4
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
        public static string[] list_files(string directory_path, bool printToConsole)
        {
            string[] listFiles = Directory.GetFiles(directory_path);
            if (printToConsole)
            {
                foreach (string file in listFiles)
                {
                    Debug.WriteLine(file);
                }
            }
            return listFiles;

        }


        public static long GetStorageFreeSpace(string directoryPath = "\\")
        {
            try
            {
                DriveInfo driveInfo = new DriveInfo(directoryPath);
                return driveInfo.TotalSize - GetStorageUsage(directoryPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting free space: {ex.Message}");
                return -1; // Indicate error
            }
        }

        /// <summary>
        /// Gets the total storage usage in bytes for a given directory (default: root)
        /// </summary>
        public static long GetStorageUsage(string directoryPath = "\\")
            {
                try
                {
                    if (!Directory.Exists(directoryPath))
                    {
                        Debug.WriteLine($"Directory does not exist: {directoryPath}");
                        return 0;
                    }

                    long totalSize = 0;

                    // Sum file sizes in the current directory
                    foreach (string file in Directory.GetFiles(directoryPath))
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            totalSize += fileInfo.Length;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error accessing file {file}: {ex.Message}");
                        }
                    }

                    // Recursively sum subdirectories
                    foreach (string dir in Directory.GetDirectories(directoryPath))
                    {
                        try
                        {
                            totalSize += GetStorageUsage(dir);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error accessing directory {dir}: {ex.Message}");
                        }
                    }

                    return totalSize;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error calculating storage usage: {ex.Message}");
                    return -1; // Indicate error
                }
            }

            /// <summary>
            /// Prints storage usage information for debugging
            /// </summary>
            public static void PrintStorageInfo(string directoryPath = "\\")
            {
                long usedBytes = GetStorageUsage(directoryPath);
                if (usedBytes >= 0)
                {
                    Debug.WriteLine($"Storage used in {directoryPath}: {usedBytes}B");
                    Debug.WriteLine($"({BytesToReadableFormat(usedBytes)})");
                    //Debug.WriteLine($"Free space: {GetStorageFreeSpace(directoryPath)}B");
            }
            }

            /// <summary>
            /// Converts bytes to a human-readable format (KB, MB, GB)
            /// </summary>
            private static string BytesToReadableFormat(long bytes)
            {
                string[] suffixes = { "B", "KB", "MB", "GB" };
                int suffixIndex = 0;
                double size = bytes;

                while (size >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    size /= 1024;
                    suffixIndex++;
                }

                return $"{size:0.##} {suffixes[suffixIndex]}";
            }



    /* no more used... in further implementations only the Hours, minutes and seconds will be stored, coz the year, month and day are already stored in the file name
    public static void new_formated_record(string file_path, string data)
    {
        var formated_data = $"[{DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss")}] sensor_measurement={data}\r\n";
        append(file_path, formated_data);
    }*/
    }
}