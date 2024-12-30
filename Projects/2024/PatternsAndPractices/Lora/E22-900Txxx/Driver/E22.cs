// written by Roman Kiss, December 14, 2024,
// version: 1.0.0.0  12/14/2024  initial version
//          1.0.1.0  12/26/2024  receiving packet - improvement
//          1.0.2.0  12/28/2024  new DataReceived handler 
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
        const int readBufferSize = 1024;
        const int WriteBufferSize = 512;
        const byte maxPacketLength = 240;
        const byte PacketHeaderLength = 7;
        const byte maxPacketDataLength = maxPacketLength - PacketHeaderLength;

        public event OnLoraReceived OnPacketReceived = null;

        public E22(string portname = "COM2", int baudRate = 115200, int rst = -1, int m0 = -1, int m1 = -1, int aux = -1, GpioController ioctrl = null)
        {
            serialport = new SerialPort(portname, baudRate: 115200);
            if (serialport != null)
            {
                serialport.ReadBufferSize = readBufferSize; 
                serialport.WriteBufferSize = WriteBufferSize; 
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
                // C1 ADDH ADDL LEN Byte_0...N EOD RSSI
                if (e.EventType == SerialData.WatchChar)
                {
                    int numberOfbytes = 0;
                    byte[] buffer = new byte[3];  //ADDH, ADDL, LEN  || EOD, RSSI
                    RcvLoRaArgs packet = new RcvLoRaArgs();
                    try
                    {
                     step1: //Sync on the mark 0xC1
                        numberOfbytes = modem.serialport.BytesToRead;
                        var syncbyte = modem.serialport.ReadByte();
                        Debug.Write($"LoRaRcv[{numberOfbytes}]: {DateTime.UtcNow.ToString("hh:mm:ss.fff")} Starting {syncbyte:X2}");
                        while (syncbyte != 0xC1)
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

                     step2: //Get the Header (3 bytes)
                        numberOfbytes = modem.serialport.Read(buffer, 0, 3);
                        packet.AddressID = (ushort)(((ushort)buffer[0] << 8) + buffer[1]);
                        packet.DataLength = (byte)buffer[2];

                     step3: //Get the Data
                        if (packet.DataLength > maxPacketDataLength)
                        {
                            Debug.WriteLine($"LoRaRcv: Wrong DataLength {packet.DataLength} > {maxPacketDataLength}");
                            return;
                        }
                        if (packet.DataLength > 0)
                        {
                            packet.Data = new byte[packet.DataLength];
                            numberOfbytes = modem.serialport.Read(packet.Data, 0, packet.DataLength);
                        }

                     step4: //Set the rssi byte
                        numberOfbytes = modem.serialport.Read(buffer, 0, 2);
                        packet.RSSI = buffer[1];
                        if (buffer[0] == 0x00) //EndOfData (should be changed to CRC8)
                        {
                            //step5: publishing packet to all subscribers
                            Debug.WriteLine($"LoRaRcv[{packet.DataLength}] 0x{packet.AddressID:X4} 0x{packet.RSSI:X2}");
                            Debug.WriteLine($"LoRaRcv[{modem.serialport.BytesToRead}]: Stream");
                            if (modem.OnPacketReceived != null && packet != null)
                                modem.OnPacketReceived.Invoke(modem, packet);
                        }
                        else
                        {
                            //Error handling: There is a small rcv buffer, so a simple solution is to find a next packet (note, that the 1 (2) packets can be lost)
                            Debug.WriteLine($"LoRaRcv: Wrong EndOfData 0x{buffer[0]:X2}, Find next packet ...");
                            goto step1;                            
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
                        numberOfbytes = modem.serialport.BytesToRead;
                        Debug.WriteLine($"LoRaRcv[{numberOfbytes}]: {DateTime.UtcNow.ToString("hh:mm:ss.fff")} END");
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





/*
#region Find next packet within the data
Debug.WriteLine($"LoRaRcv: Wrong EndOfData 0x{buffer[0]:X2}, Find next packet ...");
for (int ii = 0; ii < packet.DataLength; ii++)
{
    if (packet.Data[ii] == 0xC1)
    {
        if (ii++ == packet.DataLength) goto step2;
        byte rest = (byte)(packet.DataLength - ii);
        //Debug.WriteLine($"LoRaRcv[{rest}]: {BitConverter.ToString(packet.Data, ii, packet.DataLength - ii)}");
        packet.AddressID = (ushort)(((ushort)packet.Data[ii] << 8));
        if (ii++ < packet.DataLength)
        {
            packet.AddressID += packet.Data[ii];
            if (ii++ < packet.DataLength)
            {
                byte dataLength2 = (byte)packet.Data[ii];
                if (ii++ < packet.DataLength)
                {
                    
                    byte[] data2 = new byte[dataLength2];
                    if (packet.DataLength <= dataLength2)
                    {
                        //int jj = 0;
                        //while (ii < packet.DataLength)
                        //    data2[jj++] = packet.Data[ii++];
                        int jj = packet.DataLength - ii;
                        new SpanByte(packet.Data).Slice(ii).CopyTo(data2); 
                        data2[jj++] = buffer[0];  //EOD
                        data2[jj++] = buffer[1];  //RSSI
                        //Debug.WriteLine($"LoRaRcv[{jj}]: {BitConverter.ToString(data2, 0)}");
                        numberOfbytes = modem.serialport.Read(data2, jj, dataLength2 - jj);  
                        packet.DataLength = dataLength2;
                        packet.Data = data2;
                        //Debug.WriteLine($"LoRaRcv[{packet.DataLength}]: 0x{packet.AddressID:X4} {BitConverter.ToString(packet.Data, 0)}");
                        goto step4;
                    }
                    else
                    {
                        Debug.WriteLine($"LoRaRcv: Not implemented yet");
                    }
                }
                packet.DataLength = dataLength2;
            }
            else
            {
                numberOfbytes = modem.serialport.Read(buffer, 0, 1);
                packet.DataLength = (byte)buffer[0];
            }
        }
        else
        {
            numberOfbytes = modem.serialport.Read(buffer, 0, 2);
            packet.AddressID += buffer[0];
            packet.DataLength = (byte)buffer[1];
        }
        goto step3;
    }
}
Debug.WriteLine($"LoRaRcv: No packet found within received data.");
#endregion
*/
