// written by Roman Kiss, October 2nd, 2024,
// version: 1.0.0.0  02/10/2024
//
// example: https://reyax.com//upload/products_download/download_file/LoRa_AT_Command_RYLR998_RYLR498_EN.pdf
//
//
//
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace MyApp
{
    public delegate void OnReceive(object source, RcvPacketArgs e);

    // interesting event
    public class RcvPacketArgs : EventArgs
    {
        public ushort AddressID { get; set; }
        public short DataLength { get; set; }
        public string Data { get; set; }
        public short RSSI { get; set; }
        public short SNR { get; set; }
        public string Packet { get; set; }
    }

    public class RYLR998
    {
        SerialPort serialport = null;
        GpioPin rstLine = null;
        GpioController ioctrl = null;
        string _response = string.Empty;
        readonly object _sendLock = new();
        AutoResetEvent are = new AutoResetEvent(false);

        public event OnReceive OnPacketReceived = null;

        public RYLR998(string portname = "COM2", int baudRate = 115200, int rst = -1, GpioController ioctrl = null)
        {
            serialport = new SerialPort(portname, baudRate: 115200);
            if (serialport != null)
            {
                serialport.ReadBufferSize = 320;
                serialport.WriteBufferSize = 320;
                serialport.ReadTimeout = 5000;
                serialport.WatchChar = '+';
                if (rst >= 0)
                {
                    this.ioctrl = ioctrl;
                    ioctrl ??= new GpioController();
                    rstLine = ioctrl.OpenPin(rst, PinMode.Output);
                }
            }
        }

        public void Close()
        {
            Reset(bPullUp: false);
            Debug.WriteLine($"Modem: Closing and Disposing");
            serialport?.Close();
            serialport?.Dispose();
            serialport = null;
        }

        public bool IsOpen => serialport != null ? serialport.IsOpen : false;

        public void Reset(bool bPullUp = true)
        {
            if (rstLine != null)
            {
                Debug.WriteLine($"Modem: cold reset ...");
                rstLine.Write(PinValue.High);
                Thread.Sleep(100);
                rstLine.Write(PinValue.Low);
                Thread.Sleep(200);
                if (bPullUp)
                {
                    rstLine.Write(PinValue.High);
                    Thread.Sleep(100);
                }
            }
        }

        public static RYLR998 Create(string portname = "COM2", ushort loraAddress = 0, int rst = -1, GpioController ioctrl = null, int baudRate = 115200)
        {

            var modem = new RYLR998(portname, baudRate, rst, ioctrl);
            modem.serialport.Open();
            Debug.WriteLine("SerialPort is opened.");

            // reset modem
            int retry = 3;
            do
            {
                modem.Reset();
                Thread.Sleep(100);
                string rcv = modem.serialport.ReadExisting();
                Debug.WriteLine($"Modem: PWR/Reset response = {rcv}");
                if (string.IsNullOrEmpty(rcv)) break;
                if (rcv.IndexOf("+READY") >= 0) break;
                if (retry-- < 0) goto CloseAndExit;
            } while (true);

            // internal subscriber
            modem.serialport.DataReceived += (source, e) =>
            {
                try
                {
                    if (e.EventType == SerialData.WatchChar) // +
                    {
                        string packet = modem.serialport.ReadLine();
                        Debug.WriteLine($"LoRaEvent: {packet}");
                        if (packet.StartsWith("+RCV"))
                        {
                            modem.OnPacketReceived?.Invoke(modem, PacketRcvParser(packet));
                        }
                        else
                        {
                            modem._response = packet;
                            modem.are.Set();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LoRaEvent: {ex.Message}");
                }
            };

            // now the modem is after a cold reset and it should be work based on the AT commands
            Thread.Sleep(100);
            if (!modem.WriteCommand("AT", retry: 3)) goto CloseAndExit;
            if (modem.rstLine == null)
            {
                // try to make a soft reset 
                if (!modem.WriteCommand("AT+RESET", "+READY")) goto CloseAndExit;
            }
            if (!modem.WriteCommand("AT+BAND=868500000")) goto CloseAndExit;
            Thread.Sleep(10);
            if (modem.WriteCommand($"AT+ADDRESS={loraAddress}")) return modem;

            CloseAndExit:
            modem.Close();
            return null;
        }

        public bool WriteCommand(string cmd, string rsp = "+OK", int retry = 3)
        {
            if (serialport == null) return false;
            if (!serialport.IsOpen)
            {
                Debug.WriteLine("SerialPort is not opened");
                return false;
            }
            try
            {
                do
                {
                    // only one command at the time to avoid +ERR=17 (Last TX was not completed)
                    lock (_sendLock)
                    {
                        //are.Reset();
                        Debug.WriteLine($"LoRa: {cmd}");
                        serialport.WriteLine(cmd + "\r");
                        are.WaitOne(5000, false);
                        if (_response.StartsWith(rsp)) return true;
                    }
                } while (retry-- > 0);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static RcvPacketArgs PacketRcvParser(string packet)
        {
            if (string.IsNullOrEmpty(packet)) return null;
            packet = packet.TrimEnd('\r', '\n');
            string[] rcv = packet.Split(new char[] { '=' }, 2);
            if (rcv.Length != 2) return null;
            string[] args = rcv[1].Split(new char[] { ',' }, 5);
            if (args.Length != 5) return null;

            ushort.TryParse(args[0], out ushort addr);
            byte.TryParse(args[1], out byte length);
            short.TryParse(args[3], out short rssi);
            short.TryParse(args[4], out short snr);

            return new RcvPacketArgs()
            {
                AddressID = addr,
                DataLength = length,
                Data = args[2],
                RSSI = rssi,
                SNR = snr,
                Packet = packet
            };
        }

        public bool Send(ushort address = 0, string data = "Ping")
        {
            if (data == null || data.Length > 240) return false;
            return this.WriteCommand($"AT+SEND={address},{data.Length},{data}");
        }

        public void SendAsync(ushort address = 0, string data = "Ping")
        {
            var task = new Thread(() =>
            {
                Send(address, data);
            })
            { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }
    }
}
