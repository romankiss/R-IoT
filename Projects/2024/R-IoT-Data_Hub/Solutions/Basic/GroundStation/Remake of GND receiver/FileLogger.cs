using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class FileLogger
{
    private readonly string _logFilePath;
    private readonly string _csvFilePath;

    public FileLogger(string logFilePath, string csvFilePath, string fileHeader)
    {
        _logFilePath = logFilePath;
        _csvFilePath = csvFilePath;
        InitializeFiles(fileHeader);
    }

    private void InitializeFiles(string fileHeader)
    {
        if (!File.Exists(_csvFilePath) && _csvFilePath != null)
        {
            File.WriteAllText(_csvFilePath, fileHeader);
        }
        if(!File.Exists(_logFilePath) && _logFilePath != null)
        {
            File.WriteAllText(_logFilePath, fileHeader);
        }
    }

    public async Task LogToFileAsync(string message)
    {
        if (_logFilePath == null)
        {
            Debug.WriteLine("Log file path is not set.");
            return;
        }
        await File.AppendAllTextAsync(_logFilePath, $"{DateTime.Now}: {message}\n");
    }

    public async Task LogToCsvAsync(Dictionary<string, string> telemetryData)
    {
        if (_csvFilePath == null)
        {
            Debug.WriteLine("CSV file path is not set.");
            return;
        }
        string csvLine = string.Join(",", telemetryData.Values) + "\n";
        try
        {
            await File.AppendAllTextAsync(_csvFilePath, csvLine);

        }
        catch(Exception ex)
        {
            Debug.WriteLine("Ex thrown while acessing file: " + ex.Message);
        }
    }
}