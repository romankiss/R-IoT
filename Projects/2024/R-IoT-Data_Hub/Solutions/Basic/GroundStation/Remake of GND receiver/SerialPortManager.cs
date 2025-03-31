using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Diagnostics;

public class SerialPortManager : IDisposable
{
    private SerialPort _serialPort;
    private readonly object _sendLock = new();

    public event EventHandler<byte[]> DataReceived;
    public event EventHandler<string> ErrorOccurred;

    public bool IsConnected => _serialPort?.IsOpen ?? false;

    public async Task ConnectAsync(string portName, int baudRate)
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();

            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
            };

            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to connect: {ex.Message}");
            throw;
        }
    }

    public void Disconnect()
    {
        if (_serialPort?.IsOpen == true)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
        }
    }

    public string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public async Task SendAsync(byte[] data)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Serial port not connected.");

        lock (_sendLock)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
                Debug.WriteLine($"Sent: {BitConverter.ToString(data)}");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to send data: {ex.Message}");
                throw;
            }
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            byte[] buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, buffer.Length);
            
            DataReceived?.Invoke(this, buffer);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error reading data: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}