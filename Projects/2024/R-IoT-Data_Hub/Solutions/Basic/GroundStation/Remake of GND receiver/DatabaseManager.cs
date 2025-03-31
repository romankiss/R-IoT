using Npgsql;
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
            INSERT INTO sensor_data (temperature, humidity, pressure, distance, counter)
            VALUES (@temperature, @humidity, @pressure, @distance, @counter)";

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@temperature", float.Parse(telemetryData["T"]));
            cmd.Parameters.AddWithValue("@humidity", float.Parse(telemetryData["H"]));
            cmd.Parameters.AddWithValue("@pressure", int.Parse(telemetryData["P"]));
            cmd.Parameters.AddWithValue("@distance", int.Parse(telemetryData["D"]));
            cmd.Parameters.AddWithValue("@counter", int.Parse(telemetryData["#"]));

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            throw;
        }
    }
}