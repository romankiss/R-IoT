// written by Roman Kiss, December 14, 2024,
// version: 1.0.0.0  12/14/2024  initial version
//          1.0.1.0  12/26/2024  receiving packet - improvement
//          1.0.2.0  12/28/2024  new DataReceived handler 
//          1.0.3.0  12/31/2024  new DataReceived handler 
//          1.0.3.1  01/01/2025  fixing bugs in the DataReceived handler 
//
// Note: This version handles only a Tx/RX mode, such as the M1=0 M0=0
//
//
//
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;


namespace NFAppAtomS3_MQTT
{
    public delegate void OnLoraReceived(object source, RcvLoRaArgs e);

    public class RcvLoRaArgs : EventArgs
    {
        public ushort AddressID { get; set; }
        public short DataLength { get; set; }
        public byte[] Data { get; set; }
        public short RSSI { get; set; }
    }
    public class E22
    {
        SerialPort serialport = null;
        GpioPin rstLine = null;
        GpioController ioctrl = null;
        string _response = string.Empty;
        readonly object _sendLock = new();
        AutoResetEvent are = new AutoResetEvent(false);
        ushort own_address = 0;
        const int readBufferSize = 512;
        const int writeBufferSize = 512;
        const byte maxLoRaPacketLength = 240;   // based on the spec
        const byte sendPacketHeaderLength = 7;  // toADDH, toADDL, Channel, SOH, fromADDH, fromADDL, LEN
        const byte rcvPacketHeaderLength = 3;   // fromADDH, fromADDL, LEN
        const byte maxPacketDataLength = maxLoRaPacketLength - rcvPacketHeaderLength;
        const byte SOH = 0xC1; // SOH - StartOfHeading  0xC1 (0x01)
        const byte EOD = 0x00; // EOD - EndOfData
        //static byte[] buffer = new byte[maxLoRaPacketLength];   // rcv packet buffer
        static byte[] buffer = new byte[readBufferSize];   // rcv packet buffer

        public event OnLoraReceived OnPacketReceived = null;

        public E22(string portname = "COM2", int baudRate = 115200, int rst = -1, int m0 = -1, int m1 = -1, int aux = -1, GpioController ioctrl = null)
        {
            serialport = new SerialPort(portname, baudRate: 115200);
            if (serialport != null)
            {
                serialport.ReadBufferSize = readBufferSize; 
                serialport.WriteBufferSize = writeBufferSize; 
                serialport.ReadTimeout = 5000;
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
                // C1 ADDH ADDL LEN Byte_0...N EOD RSSI
                int numberOfbytes = modem.serialport.BytesToRead;
                if (numberOfbytes > 0)
                {
                    try
                    {
                        //step1: Sync on the mark 0xC1
                        var syncbyte = modem.serialport.ReadByte();
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
                            syncbyte = modem.serialport.ReadByte();
                            Debug.Write($".{syncbyte:X2}");
                        }
                        Debug.WriteLine("");

                        //step2: Get the Header (3 bytes)
                        numberOfbytes = modem.serialport.Read(buffer, 0, rcvPacketHeaderLength);
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
                            numberOfbytes = modem.serialport.Read(buffer, rcvPacketHeaderLength, dataLength + 2); // EOD + RSSI
                        }

                        //step4: Set the rssi byte
                        byte eod = buffer[rcvPacketHeaderLength + dataLength];
                        byte rssi = buffer[rcvPacketHeaderLength + dataLength + 1];
                        // simple check if we have EOD in the position, later this check will replaced CRC8 calculation
                        if (eod == EOD) //EndOfData
                        {
                            //step5: publishing packet to all subscribers
                            Debug.WriteLine($"LoRaRcv[{dataLength}] 0x{addressId:X4} 0x{rssi:X2}");
                            Debug.WriteLine($"LoRaRcv[{modem.serialport.BytesToRead}]: Stream");
                            if (modem.OnPacketReceived != null)
                            {
                                var packet = new RcvLoRaArgs()
                                {
                                    AddressID = addressId,
                                    DataLength = dataLength,
                                    Data = dataLength == 0 ? null : new SpanByte(buffer, rcvPacketHeaderLength, dataLength).ToArray(),
                                    RSSI = rssi
                                };
                                modem.OnPacketReceived.Invoke(modem, packet);
                            }
                        }
                        else
                        {
                            //Error exit
                            Debug.WriteLine($"LoRaRcv: Wrong EndOfData 0x{eod:X2}, Find next packet ...");
                        }
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine($"LoRaRcv: Timeout to read requested number of bytes");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LoRaRcv: {ex.InnerException?.Message ?? ex.Message}");
                    }
                    finally
                    {
                        // step6: Ending (number of bytes remaining in the stream)
                        Debug.WriteLine($"nFmem_LoRaRcv={nanoFramework.Runtime.Native.GC.Run(true)}");
                        Debug.WriteLine($"LoRaRcv[{modem.serialport.BytesToRead}]: {DateTime.UtcNow.ToString("hh:mm:ss.fff")} END");
                        Debug.WriteLine("");
                    }
                }
            };
            return modem;
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

                byte[] bytes = new byte[header.Length + datalength + 1];
                header.CopyTo(bytes, 0);
                data?.CopyTo(bytes, header.Length);
                bytes[header.Length + datalength] = 0x00; // End of Data

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
