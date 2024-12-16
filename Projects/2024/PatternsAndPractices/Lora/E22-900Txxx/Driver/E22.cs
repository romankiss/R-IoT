// written by Roman Kiss, December 14, 2024,
// version: 1.0.0.0  12/14/2024  initial version
//
// Note: This version handles only a Tx/RX mode, such as the M1=0 M0=0
//
//
//
using nanoFramework.Presentation.Media;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace NFAppAtomS3_MQTT
{
    public class E22
    {
        SerialPort serialport = null;
        GpioPin rstLine = null;
        GpioController ioctrl = null;
        string _response = string.Empty;
        readonly object _sendLock = new();
        AutoResetEvent are = new AutoResetEvent(false);
        ushort own_address = 0;
        const int bufferSize = 320;

        public event OnReceive OnPacketReceived = null;

        public E22(string portname = "COM2", int baudRate = 115200, int rst = -1, int m0 = -1, int m1 = -1, int aux = -1, GpioController ioctrl = null)
        {
            serialport = new SerialPort(portname, baudRate: 115200);
            if (serialport != null)
            {
                serialport.ReadBufferSize = bufferSize; 
                serialport.WriteBufferSize = bufferSize; 
                serialport.ReadTimeout = 5000;
                serialport.WatchChar =  (char)0xC1;
            }
        }

        public void Close()
        {
            Debug.WriteLine($"Modem: Closing and Disposing");
            serialport?.Close();
            serialport?.Dispose();
            serialport = null;
        }

        public bool IsOpen => serialport != null ? serialport.IsOpen : false;

        public static E22 Create(string portname = "COM2", ushort loraAddress = 0, int rst = -1, int m0 = -1, int m1 = -1, int aux = -1, GpioController ioctrl = null, int baudRate = 115200)
        {
            var modem = new E22(portname, baudRate, rst, m0, m1, aux, ioctrl);
            modem.own_address = loraAddress;
            modem.serialport.Open();
            Debug.WriteLine("SerialPort is opened.");

            // internal subscriber
            modem.serialport.DataReceived += (source, e) =>
            {
                int numberOfbytes = 0;
                byte[] buffer = new byte[bufferSize];
                try
                {
                    // C1 ADDH ADDL LEN Byte_0...N RSSI
                    if (e.EventType == SerialData.WatchChar)
                    {
                        // Simple solution, based on the Watch Character such as 'C1'. Note, that this part must be improved!  
                        Thread.Sleep(10);
                        numberOfbytes = modem.serialport.BytesToRead;
                        if (numberOfbytes > 0)
                        {
                            modem.serialport.Read(buffer, 0, numberOfbytes);
                            var packet = PacketRcvParser(buffer, numberOfbytes);
                            Debug.WriteLine($"LoRaRcv[{numberOfbytes}]: {packet?.Packet ?? (numberOfbytes != 0 ? BitConverter.ToString(buffer, 0, numberOfbytes) : "(empty)")}");
                            if (modem.OnPacketReceived != null && packet != null)
                                modem.OnPacketReceived.Invoke(modem, packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LoRaRcv[{numberOfbytes}]: {ex.InnerException?.Message ?? ex.Message}");
                }
            };
            return modem;
        }

        public static RcvPacketArgs PacketRcvParser(byte[] packet, int length)
        {
            if ( packet == null) return null;
            if (packet[0] != 0xC1) return null;  // signature 0xC1
            if (length < 4) return null;
            if (length != packet[3] + 5) return null; // C1, addH, addl, lendata, ..data..., rssi  

            ushort addrSender = (ushort)(((ushort)packet[1] << 8) + packet[2]);
            byte lendata = (byte)packet[3];
            string raw = new StringBuilder(BitConverter.ToString(packet, 0, length)).Replace("-", "").ToString();
            string data = raw.Substring(8, (byte)packet[3] * 2);
            short rssi = (short)packet[length - 1];

            return  new RcvPacketArgs() { AddressID = addrSender, DataLength = lendata, Data = data, RSSI = rssi, Packet = raw };
        }

        public bool Send(ushort address = 0xFFFF, byte[] data = null, byte channel = 0x12)
        {
            byte[] header = new byte[7];

            try
            {
                if (serialport == null || !serialport.IsOpen) return false;
                if (data != null && data.Length > 197) return false;
                byte datalength = (byte)(data == null ? 0 : data.Length);
                if (header.Length + datalength > 200) return false;

                header[0] = (byte)((address >> 8) & 0xFF);
                header[1] = (byte)(address & 0xFF);
                header[2] = channel;    // channel
                header[3] = 0xC1;       // start mark
                header[4] = (byte)((own_address >> 8) & 0xFF);
                header[5] = (byte)(own_address & 0xFF);
                header[6] = datalength;

                byte[] bytes = new byte[header.Length + datalength];
                header.CopyTo(bytes, 0);
                data?.CopyTo(bytes, header.Length);

                lock (_sendLock)
                {
                    Debug.WriteLine($"LoRaSend[{bytes.Length}]: {BitConverter.ToString(bytes)}");
                    serialport.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"LoRaSend: {ex.Message}");
                return false;
            }
        }

        public bool Send(ushort address = 0xFFFF, string data = "Ping", byte channel = 0x12)
        {
            return Send(address, Encoding.UTF8.GetBytes(data ??= ""), channel);
        }

        public void SendAsync(ushort address = 0xFFFF, byte[] data = null, byte channel = 0x12)
        {
            var task = new Thread(() =>
            {
                Send(address, data, channel);
            })
            { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }
        public void SendAsync(ushort address = 0xFFFF, string data = "Ping", byte channel = 0x12)
        {
            SendAsync(address, Encoding.UTF8.GetBytes(data ??= ""), channel);
        }
    }
}
