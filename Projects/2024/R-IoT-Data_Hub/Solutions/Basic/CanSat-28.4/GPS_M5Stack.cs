// GPS Reader for GNGGA data
// written by Roman Kiss, March 27, 2025,
// version: 1.0.0.0  3/27/2025  initial version
//
//
//
//
//
//using NFAppLoRaE22;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace CanSat-28.4
{
    public delegate void OnGpsReceived(object source, RcvGpsArgs e);
    public delegate void OnGpsUnknownReceived(object source, RcvGpsArgs e);

    public class RcvGpsArgs : EventArgs
    {
        public string data { get; set; }
    }

    public class GPS
    {
        SerialPort _serialport = null;
        static string _rcvdata = string.Empty;
        static readonly object _rcvLock = new();
        //
        const int readBufferSize = 512;
        const int readTimeout = 6000;
        const char watchChar = '$';
        const int receivedBytesThreshold = 6;
        //
        public event OnGpsReceived OnGpsReceived_GNGGA = null;
        public event OnGpsUnknownReceived OnGpsReceived_Unknown = null;

        public bool IsOpen => _serialport != null ? _serialport.IsOpen : false;

        protected GPS(string portname = "COM1", int baudRate = 9600)
        {
            _serialport = new SerialPort(portname, baudRate: baudRate);
            if (_serialport != null)
            {
                _serialport.ReadBufferSize = readBufferSize;
                _serialport.ReadTimeout = readTimeout;
                _serialport.WatchChar = watchChar;
                _serialport.ReceivedBytesThreshold = receivedBytesThreshold;
            }
        }

        public void Close()
        {
            Debug.WriteLine($"GPS: Closing and Disposing");
            _serialport?.Close();
            _serialport?.Dispose();
            _serialport = null;
        }

        public static GPS Create(string portname = "COM1", int baudRate = 115200)
        {
            var gps = new GPS(portname, baudRate);
            if (gps != null)
            {
                gps._serialport.DataReceived += (s, e) =>
                {
                    if (e.EventType == SerialData.WatchChar)
                    {
                        string str = gps._serialport.ReadLine();
                        if (str.StartsWith("$GNGGA"))
                        {
                            //Debug.WriteLine(str);
                            lock (_rcvLock)
                            {
                                _rcvdata = str.Substring(0);
                            }
                            gps.OnGpsReceived_GNGGA?.Invoke(gps, new RcvGpsArgs() { data = str.Substring(0) });
                        }
                        else
                        {
                            gps.OnGpsReceived_Unknown?.Invoke(gps, new RcvGpsArgs() { data = str.Substring(0) });
                        }
                    }
                };
                gps._serialport.Open();
                Debug.WriteLine("GPS: SerialPort is opened.");
            }
            return gps;
        }

        // https://openrtk.readthedocs.io/en/latest/communication_port/nmea.html
        public bool TryParseGNGGA(out float latitude, out float longitude, out float altitude, string text = null)
        {
            latitude = longitude = altitude = 0.0F;
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_rcvdata))
            {
                lock (_rcvLock)
                {
                    text = _rcvdata.Substring(0);
                    _rcvdata = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(text) || !text.StartsWith("$GNGGA"))
                return false;

            Debug.Write($"{text}");

            var parts = text.Split(',');
            if (!float.TryParse(parts[2], out latitude))
                return false;
            if (!float.TryParse(parts[4], out longitude))
                return false;
            if (!float.TryParse(parts[9], out altitude))
                return false;

            int latDD = (int)latitude / 100;
            float latMM = latitude - latDD * 100;
            latitude = latDD + latMM / 60;
            if (parts[3] == "S") latitude = -latitude;

            int lonDD = (int)longitude / 100;
            float lonMM = longitude - lonDD * 100;
            longitude = lonDD + lonMM / 60;
            if (parts[5] == "W") longitude = -longitude;

            return true;
        }
    }
}