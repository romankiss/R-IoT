using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class SerialPortManager : IDisposable
{
    const int SOH = 0xC1;
    const int rcvPacketHeaderLength = 3;
    const int maxPacketDataLength = 128;
    const int packetTrailerLength = 4;// \r\n bytes + checksum bytes = 4 bytes total
    private SerialPort _serialPort;
    private readonly object _sendLock = new();

    public event EventHandler<byte[]> DataReceived;
    public event EventHandler<string> ErrorOccurred;


    private readonly List<byte> _readBuffer = new List<byte>();
    private readonly byte[] _messageDelimiter = Encoding.ASCII.GetBytes("\n"); // Adjust delimiter if needed (e.g., `\r\n`)

    private DateTime _lastReceivedTime = DateTime.MinValue;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);
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
            // 1. Get the total bytes available
            int bytesAvailable = _serialPort.BytesToRead;
            if (bytesAvailable <= 0) return;

            // 2. Read all available bytes at once
            byte[] buffer = new byte[bytesAvailable];
            int bytesRead = _serialPort.Read(buffer, 0, bytesAvailable);

            // 3. Process the received data
            ProcessReceivedData(buffer, bytesRead);
        }
        catch (InvalidOperationException ex)
        {
            ErrorOccurred?.Invoke(this, $"Port not open: {ex.Message}");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error reading data: {ex.Message}");
        }
    }

    private void ProcessReceivedData(byte[] buffer, int bytesRead)
    {
        // 1. Add new data to the buffer
        lock (_readBuffer)
        {
            _readBuffer.AddRange(buffer.Take(bytesRead));
        }

        // 2. Process complete packets
        while (TryExtractPacket(out byte[] packet))
        {
            DataReceived?.Invoke(this, packet);
        }
    }

    private bool TryExtractPacket(out byte[] packet)
    {
        packet = null;
        lock (_readBuffer)
        {
            // 1. Find start of packet (0xC1)
            int startIndex = _readBuffer.IndexOf(SOH);
            if (startIndex < 0) return false;

            // Remove any garbage before the start marker
            if (startIndex > 0)
            {
                _readBuffer.RemoveRange(0, startIndex);
                Debug.WriteLine($"Removed {startIndex} bytes of preamble");
            }

            // 2. Check if we have enough data for header
            if (_readBuffer.Count < rcvPacketHeaderLength + 1) // +1 for SOH
                return false;

            // 3. Parse header (after SOH)
            ushort addressId = (ushort)((_readBuffer[1] << 8) + _readBuffer[2]);
            byte dataLength = _readBuffer[3];

            // 4. Check if we have complete packet
            int totalPacketLength = 1 + rcvPacketHeaderLength + dataLength + 2; // SOH + header + data + footer
            if (_readBuffer.Count < totalPacketLength)
                return false;

            // 5. Extract the complete packet
            packet = _readBuffer.GetRange(1, totalPacketLength-packetTrailerLength).ToArray();
            _readBuffer.RemoveRange(0, totalPacketLength);

            // 6. Validate packet
            if (dataLength > maxPacketDataLength)
            {
                Debug.WriteLine($"Invalid packet length: {dataLength}");
                return false;
            }

            return true;
        }
    }


    public void Dispose()
    {
        Disconnect();
    }
}

