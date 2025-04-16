using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remake_of_GND_receiver
{
    public class ConnStrGatherer
    {
        private readonly string _filePath;

        public ConnStrGatherer(string pathToTXTFileContainingConnStr = "C:\\Users\\ratze\\Desktop\\ConnectionStringAWSPostgreDBCanSat.txt")
        {
            // Expand the ~ to the user's home directory
            _filePath = Environment.ExpandEnvironmentVariables(pathToTXTFileContainingConnStr);
            Debug.WriteLine($"Connection string file path: {_filePath}");
        }

        /// <summary>
        /// Reads the connection string from the specified text file
        /// </summary>
        /// <returns>The connection string</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        /// <exception cref="IOException">Thrown when there's an error reading the file</exception>
        public string GetConnectionString()
        {
            try
            {
                // Read all text from the file and trim any whitespace
                return File.ReadAllText(_filePath).Trim();
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                throw new FileNotFoundException($"Could not find the connection string file at: {_filePath}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading the connection string from file: {_filePath}", ex);
            }
        }
    }
}