using System;
using System.Threading;
using System.Diagnostics;

using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
//using CanSat.E22_900T22D.Wrapper;
//using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;



using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
//using System.Net;
//using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
//using CanSat.E22_900T22D.Extensions;
using nanoFramework.Runtime.Native;//seems to not require, but had to install in order to remove an error



namespace FakeDataFiring
{
    public class Program
    {

        const string deviceId = "device1.00";
        const ushort loraAddress = 0x1234;
        const byte loraNetworkId = 0x12;  // 850.125 + 18 = 868.125Mhz
        const int pinButton = 41;
        const int pinNeo = 35;
        const int pinNeoPower = -1;
        const int pinLedA = -1;
        const int pinI2C1_SDA = 2;      // Grove
        const int pinI2C1_SCK = 1;      // Grove
        const int pinCOM2_TX = 6;       // HAT-G6, PORTC-G6,   G33, Grove-G2,  
        const int pinCOM2_RX = 5;       // HAT-G8, PORTC-G5,   G19, Grove-G1,  
        const int pinCOM1_TX = 8;       // PORTB-G8, 
        const int pinCOM1_RX = 7;       // PORTB-G7
        static ushort BroadcastAddress = 0xFFFF;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");


            #region LoRa E22           
            Configuration.SetPinFunction(pinCOM2_TX, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(pinCOM2_RX, DeviceFunction.COM2_RX);
            var lora = E22.Create("COM2", loraAddress: loraAddress);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    Debug.WriteLine(e.Data.ToString());
                    //Blink.Blinks(0, 255, 0, 500, 1, 1);  // it must be async call                 
                };
            }
            #endregion
            while(true)
            {
                if (lora != null && lora.IsOpen)
                {
                    lora.Send(address: BroadcastAddress, data: "T25.12H45.13P998D100");
                }
                else
                    Debug.WriteLine($"Device is not ready to publish message");
                Thread.Sleep(1000);
            }
            

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}



// written by Roman Kiss, January 3, 2025
// version: 1.0.0.0  01/03/2025  initial version
//
// Useful extensions for string, array, etc.
//


namespace FakeDataFiring
{
    /// <summary>
    /// Based on the https://err.se/crc8-for-ibutton-in-c/
    /// </summary>
    public static class Definitions
    {
        public static readonly byte[] CrcTable = new byte[] {
            0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126,
            32, 163, 253, 31, 65, 157, 195, 33, 127, 252, 162,
            64, 30, 95, 1, 227, 189, 62, 96, 130, 220, 35,
            125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3,
            128, 222, 60, 98, 190, 224, 2, 92, 223, 129, 99,
            61, 124, 34, 192, 158, 29, 67, 161, 255, 70, 24,
            250, 164, 39, 121, 155, 197, 132, 218, 56, 102,
            229, 187, 89, 7, 219, 133, 103, 57, 186, 228, 6,
            88, 25, 71, 165, 251, 120, 38, 196, 154, 101, 59,
            217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198,
            152, 122, 36, 248, 166, 68, 26, 153, 199, 37, 123,
            58, 100, 134, 216, 91, 5, 231, 185, 140, 210, 48,
            110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113,
            147, 205, 17, 79, 173, 243, 112, 46, 204, 146,
            211, 141, 111, 49, 178, 236, 14, 80, 175, 241, 19,
            77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82,
            176, 238, 50, 108, 142, 208, 83, 13, 239, 177,
            240, 174, 76, 18, 145, 207, 45, 115, 202, 148, 118,
            40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55,
            213, 139, 87, 9, 235, 181, 54, 104, 138, 212, 149,
            203, 41, 119, 244, 170, 72, 22, 233, 183, 85, 11,
            136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246,
            168, 116, 42, 200, 150, 21, 75, 169, 247, 182, 232,
            10, 84, 215, 137, 107, 53
        };
    }


    public static class ArrayExtension
    {
        public static byte[] Add(this byte[] first, byte[] second)
        {
            byte[] concat = new byte[first.Length + second.Length];
            first.CopyTo(concat, 0);
            second.CopyTo(concat, first.Length);
            return concat;
        }

        public static byte[] AddByte(this byte[] first, byte value)
        {
            byte[] concat = new byte[first.Length + 1];
            first.CopyTo(concat, 0);
            concat[first.Length] = value;
            return concat;
        }

        public static byte[] AddInt(this byte[] first, int value, bool BigEndian = true)
        {
            var arrayOfValue = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian && BigEndian)
                arrayOfValue.Reverse();
            return first.Add(arrayOfValue);
        }

        // https://stackoverflow.com/questions/6088287/reverse-an-array-without-using-array-reverse
        public static byte[] Reverse(this byte[] arr)
        {
            for (int i = 0; i < arr.Length / 2; i++)
            {
                var tmp = arr[i];
                arr[i] = arr[arr.Length - i - 1];
                arr[arr.Length - i - 1] = tmp;
            }
            return arr;
        }

        /// <summary>
        /// Based on the https://err.se/crc8-for-ibutton-in-c/
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="crc"></param>
        /// <returns></returns>
        public static byte ComputeChecksum(this byte[] bytes, byte crc = 0)
        {
            return bytes.ComputeChecksum(0, (uint)bytes.Length, crc);
        }
        public static byte ComputeChecksum(this byte[] bytes, uint start, uint length, byte crc = 0x00)
        {
            uint numberOfBytes = start + length;
            if (bytes != null && numberOfBytes <= bytes.Length)
            {
                for (uint ii = start; ii < numberOfBytes; ii++)
                {
                    crc = Definitions.CrcTable[crc ^ bytes[ii]];
                }
            }
            return crc;
        }
    }

    public static class NumericExtensions
    {
        public static int MinValue(this int value, int minValue)
        {
            return value < minValue ? minValue : value;
        }

        public static int MaxValue(this int value, int maxValue)
        {
            return value < maxValue ? value : maxValue;
        }
    }

    public static class StringExtensions
    {
        public static string Replace(this string text, string oldText, string newText)
        {
            var sb = new StringBuilder(text).Replace(oldText, newText);
            return sb.ToString();
        }
        public static string ReplaceCRLF(this string text, string replacement = null)
        {
            if (replacement == null)
                return text.Replace("\r", "\\r").Replace("\n", "\\n");
            else if (text.IndexOf("\r\n") >= 0)
                return text.Replace("\r\n", replacement);
            else
                return text.Replace("\r", replacement);
        }

        public static string IfNullOrEmpty(this string text, string newText)
        {
            return string.IsNullOrEmpty(text) ? newText : text;
        }

        // at list one item in the list must be in the text
        public static bool ContainsIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) >= 0)
                    return true;
            }
            return false;
        }

        // any item in the list must be not in the text
        public static bool NotContainsIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) >= 0)
                    return false;
            }
            return true;
        }

        // all items in the list must be in the text
        public static bool ContainsAllIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) == -1)
                    return false;
            }
            return true;
        }

        public static string UrlDecode(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Replace("%2F", "/").Replace("%3D", "=").Replace("%24", "$").Replace("%20", " ");
        }

        public static string ReadLine(this string text, string crlf = "\r\n")
        {
            return text.TrimStart(new char[] { '\r', '\n' }).Split(new char[] { '\r' }, 2)[0];
        }

        public static string Center(this string text, int width = 14)
        {
            return text.PadLeft((width + text.Length) / 2).PadRight(width);
        }
        public static string Prefix(this string text, char ch, bool flag = true)
        {
            return flag ? ch + text.Substring(1) : text;
        }
        public static string Postfix(this string text, char ch, bool flag = true)
        {
            return flag ? text.Substring(0, text.Length - 1) + ch : text;
        }
        public static string Truncate(this string text, int maxchars = 50)
        {
            return text != null && text.Length <= maxchars ? text : text.Substring(0, maxchars) + "...";
        }
        public static bool IsEven(this string text)
        {
            return text.Length % 2 == 0;
        }
        public static byte[] ToByteArray(this string text)
        {
            if (text == null || !text.IsEven())
                return null;

            int NumberChars = text.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            return bytes;
        }
        public static string FromHexString(this string text, bool checkForJson = true)
        {
            if (checkForJson && text.StartsWith("7B") && text.EndsWith("7D"))
            {
                byte[] bytes = text.ToByteArray();
                return bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            return null;
        }
        public static bool IsJson(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string input = text.Trim();
            return input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]");
            //return text.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }


    }

    public static class ThreadExtension
    {
        public static void AbortIfRunning(this Thread thread)
        {
            if (thread != null && thread.ThreadState == ThreadState.Running)
                thread.Abort();
        }
    }
}




// written by Roman Kiss, December 14, 2024,
// version: 1.0.0.0  12/14/2024  initial version
//          1.0.1.0  12/26/2024  receiving packet - improvement
//          1.0.2.0  12/28/2024  new DataReceived handler 
//          1.0.3.0  12/31/2024  new DataReceived handler 
//          1.0.3.1  01/01/2025  fixing bug in the DataReceived handler 
//          1.0.4.0  01/04/2025  adding crc8 code
//
// Note: This version handles only a Tx/RX mode, such as the M1=0 M0=0
//
//Slightly edited by Marek-Maker on jan. 20., 2025 -jic
//

namespace FakeDataFiring
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

                        //step4: check if there is a subcriber
                        if (modem.OnPacketReceived != null)
                        {
                            // Set the rssi byte and check the crc if we don't have an EOD byte 
                            byte eod = buffer[rcvPacketHeaderLength + dataLength];
                            byte rssi = buffer[rcvPacketHeaderLength + dataLength + 1];
                            byte crc8 = 0x00;
                            if (eod != EOD) //EndOfData or Crc8
                            {
                                // calculate crc8 value of the ADDH+ADDL+LEN+DATA
                                crc8 = buffer.ComputeChecksum(0, (uint)dataLength + 3);
                            }
                            // simple check if we have EOD in the position or compare calculated crc8 byte
                            if (eod == EOD || eod == crc8) //EndOfData
                            {
                                //step5: publishing packet to all subscribers
                                Debug.WriteLine($"LoRaRcv[{dataLength}] 0x{addressId:X4} rssi=0x{rssi:X2} crc=0x{eod:X2}");
                                Debug.WriteLine($"LoRaRcv[{modem.serialport.BytesToRead}]: Stream");
                                var packet = new RcvLoRaArgs()
                                {
                                    AddressID = addressId,
                                    DataLength = dataLength,
                                    Data = dataLength == 0 ? null : new SpanByte(buffer, rcvPacketHeaderLength, dataLength).ToArray(),
                                    RSSI = rssi
                                };
                                modem.OnPacketReceived.Invoke(modem, packet);
                            }
                            else
                            {
                                //Error exit
                                Debug.WriteLine($"LoRaRcv: Wrong EndOfData/CRC8 0x{eod:X2}, Find next packet ...");
                            }
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
                //bytes[header.Length + datalength] = 0x00; // End of Data
                bytes[header.Length + datalength] = bytes.ComputeChecksum(4, (uint)datalength + 3);  //ADDH+ADDL+LEN

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