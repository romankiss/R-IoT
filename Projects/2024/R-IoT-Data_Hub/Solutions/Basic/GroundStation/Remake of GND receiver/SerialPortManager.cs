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
        //byte[] line = _serialPort.ReadExisting().Select(c => (byte)c).ToArray(); option one
        //byte[] line = _serialPort.ReadLine(); //option two
        // C1 ADDH ADDL LEN Byte_0...N EOD RSSI
        byte[] buffer = new byte[256];
        int numberOfbytes = _serialPort.BytesToRead;
        if (numberOfbytes > 0)
        {
            try
            {
                //step1: Sync on the mark 0xC1
                var syncbyte = _serialPort.ReadByte();
                Debug.Write($"LoRaRcv[{numberOfbytes}]: {DateTime.UtcNow.ToString("hh:mm:ss.fff")} Starting {syncbyte:X2}");
                while (syncbyte != SOH) //0xC1)
                {
                    if (syncbyte == -1)
                    {
                        // end of the stream. no more bytes - back to the event
                        Debug.WriteLine("LoRaRcv: Didn't find the sync character 0xC1");
                        return;
                    }
                    Thread.Sleep(0);
                    syncbyte = _serialPort.ReadByte();
                    Debug.Write($".{syncbyte:X2}");
                }
                Debug.WriteLine("");

                //step2: Get the Header (3 bytes)
                numberOfbytes = _serialPort.Read(buffer, 0, rcvPacketHeaderLength);
                ushort addressId = (ushort)(((ushort)buffer[0] << 8) + buffer[1]);
                byte dataLength = buffer[2];

                //step3: Get the Data
                if (dataLength > maxPacketDataLength)
                {
                    Debug.WriteLine($"LoRaRcv: Wrong DataLength {dataLength} > {maxPacketDataLength}");
                    return;
                }
                if (dataLength >= 0)
                {
                    numberOfbytes = _serialPort.Read(buffer, rcvPacketHeaderLength, dataLength);
                    //buffer[dataLength - 1] = new byte();
                    //buffer[dataLength - 2] = new byte();
                    DataReceived?.Invoke(this, buffer);

                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error reading data: {ex.Message}");
            }
        }
    }




    public void Dispose()
    {
        Disconnect();
    }
}

