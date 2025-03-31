using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class TelemetryProcessor
{
    public Dictionary<string, string> ParseData(string rawData)
    {
        var telemetryData = new Dictionary<string, string>();

        try
        {
            var values = Regex.Split(rawData, @"([#\r\nA-Z]+)")
                             .Where(s => !string.IsNullOrEmpty(s) && s != "\r\n")
                             .ToArray();

            for (int i = 0; i < values.Length; i += 2)
            {
                if (i + 1 < values.Length)
                    telemetryData[values[i]] = values[i + 1];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing telemetry: {ex.Message}");
        }

        return telemetryData;
    }

    public bool TryExtractValue(Dictionary<string, string> data, string key, out float value)
    {
        value = 0f;
        if (data.TryGetValue(key, out string strValue))
        {
            return float.TryParse(strValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
        return false;
    }
}