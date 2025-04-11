using Npgsql;
using System.Diagnostics;
using System.Threading.Tasks;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveTelemetryAsync(Dictionary<string, string> telemetryData)
    {
        const string query = @"
            INSERT INTO telemetry.measurements (devid, temperature, humidity, pressure, distance, counter)
            VALUES (@devid, @temperature, @humidity, @pressure, @distance, @counter)";//WARNING: DevID is hardcoded to 1, change this to a variable if needed later

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            Debug.WriteLine("Connected to DB successfully");
            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@devid", int.Parse(telemetryData["I"]));
            telemetryData["T"] = telemetryData["T"].Replace(".", ","); // Replace dot with comma for decimal separator
            telemetryData["H"] = telemetryData["H"].Replace(".", ","); // Replace dot with comma for decimal separator
            telemetryData["P"] = telemetryData["P"].Replace(".", ","); // Replace dot with comma for decimal separator
            cmd.Parameters.AddWithValue("@humidity", double.Parse(telemetryData["H"]));
            cmd.Parameters.AddWithValue("@temperature", double.Parse(telemetryData["T"])); 
            cmd.Parameters.AddWithValue("@pressure", double.Parse(telemetryData["P"]));
            cmd.Parameters.AddWithValue("@distance", int.Parse(telemetryData["D"]));
            cmd.Parameters.AddWithValue("@counter", int.Parse(telemetryData["#"]));
            //WARNING: DB has set acceptable ranges for some vals like humidity(0-100 & NULL), so passing it a -1 signing in our local environment, that there was an err reading hum (defacto NULL), will caouse an err in DB INSERT
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            throw;
        }
    }
}