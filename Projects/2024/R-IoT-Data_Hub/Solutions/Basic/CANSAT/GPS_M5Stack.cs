using System;
using System.Text;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using System.IO.Ports;

namespace CanSat
{
    /// <summary>
    /// Represents the M5Stack GPS v1.1 module
    /// </summary>
    public class M5GPS
    {
        private readonly SerialPort _serialPort;
        private Thread _readingThread;
        private bool _isRunning;

        // GPS Data Properties
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Altitude { get; private set; }
        public DateTime FixTime { get; private set; }
        public int SatellitesInView { get; private set; }
        public int SatellitesInUse { get; private set; }
        public double Speed { get; private set; } // in knots
        public double Course { get; private set; } // in degrees
        public bool HasFix { get; private set; }

        // Event for when new GPS data is available
        public event EventHandler<GpsDataReceivedEventArgs> GpsDataReceived;

        /// <summary>
        /// Initializes a new instance of the M5GPS class
        /// </summary>
        /// <param name="uartPort">The UART port number (default is COM2)</param>
        /// <param name="txPin">The TX pin number (default is GPIO 17)</param>
        /// <param name="rxPin">The RX pin number (default is GPIO 16)</param>
        public M5GPS(string uartPort = "COM2", int txPin = 17, int rxPin = 16)
        {
            // Configure the UART pins
            Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);

            // Initialize the serial port
            _serialPort = new SerialPort(uartPort)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                DataBits = 8,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        /// <summary>
        /// Starts the GPS module and begins reading data
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _serialPort.Open();
            _isRunning = true;
            _readingThread = new Thread(ReadData);
            _readingThread.Start();
        }

        /// <summary>
        /// Stops the GPS module
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // Remove the nullable timeout parameter
            if (_readingThread != null)
            {
                _readingThread.Join(1000); // Use non-nullable timeout
            }

            _serialPort.Close();
        }

        private void ReadData()
        {
            StringBuilder sentenceBuilder = new StringBuilder();

            while (_isRunning)
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        char c = (char)_serialPort.ReadByte();

                        if (c == '$')
                        {
                            sentenceBuilder.Clear();
                            sentenceBuilder.Append(c);
                        }
                        else if (c == '\n')
                        {
                            string sentence = sentenceBuilder.ToString();
                            if (sentence.StartsWith("$"))
                            {
                                ProcessNmeaSentence(sentence);
                            }
                            sentenceBuilder.Clear();
                        }
                        else
                        {
                            sentenceBuilder.Append(c);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    // Ignore errors for robustness
                }
            }
        }

        private void ProcessNmeaSentence(string sentence)
        {
            string[] parts = sentence.Split(',');

            try
            {
                if (parts[0] == "$GPGGA")
                {
                    ProcessGpggaSentence(parts);
                }
                else if (parts[0] == "$GPRMC")
                {
                    ProcessGprmcSentence(parts);
                }
                else if (parts[0] == "$GPGSV")
                {
                    ProcessGpgsvSentence(parts);
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        private void ProcessGpggaSentence(string[] parts)
        {
            // $GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
            if (parts.Length < 10) return;

            // Time
            if (!string.IsNullOrEmpty(parts[1]))
            {
                try
                {
                    int time = int.Parse(parts[1]);
                    int hour = time / 10000;
                    int minute = (time % 10000) / 100;
                    int second = time % 100;

                    FixTime = new DateTime(/*
                        DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,*/
                        hour, minute, second);
                }
                catch { }
            }

            // Latitude
            if (!string.IsNullOrEmpty(parts[2]) && !string.IsNullOrEmpty(parts[3]))
            {
                try
                {
                    double lat = double.Parse(parts[2]);
                    double degrees = Math.Floor(lat / 100);
                    double minutes = lat - (degrees * 100);
                    Latitude = degrees + (minutes / 60);
                    if (parts[3] == "S") Latitude *= -1;
                }
                catch { }
            }

            // Longitude
            if (!string.IsNullOrEmpty(parts[4]) && !string.IsNullOrEmpty(parts[5]))
            {
                try
                {
                    double lon = double.Parse(parts[4]);
                    double degrees = Math.Floor(lon / 100);
                    double minutes = lon - (degrees * 100);
                    Longitude = degrees + (minutes / 60);
                    if (parts[5] == "W") Longitude *= -1;
                }
                catch { }
            }

            // Fix quality
            HasFix = parts[6] == "1";

            // Satellites in use
            if (!string.IsNullOrEmpty(parts[7]))
            {
                try
                {
                    SatellitesInUse = int.Parse(parts[7]);
                }
                catch { }
            }

            // Altitude
            if (!string.IsNullOrEmpty(parts[9]))
            {
                try
                {
                    Altitude = double.Parse(parts[9]);
                }
                catch { }
            }

            RaiseGpsDataReceivedEvent();
        }

        private void ProcessGprmcSentence(string[] parts)
        {
            // $GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
            if (parts.Length < 10) return;

            // Time and date
            if (!string.IsNullOrEmpty(parts[1]) && !string.IsNullOrEmpty(parts[9]))
            {
                try
                {
                    int time = int.Parse(parts[1]);
                    int hour = time / 10000;
                    int minute = (time % 10000) / 100;
                    int second = time % 100;

                    int date = int.Parse(parts[9]);
                    int day = date / 10000;
                    int month = (date % 10000) / 100;
                    int year = 2000 + (date % 100);

                    FixTime = new DateTime(year, month, day, hour, minute, second);
                }
                catch { }
            }

            // Status (A=active, V=void)
            HasFix = parts[2] == "A";

            // Latitude
            if (!string.IsNullOrEmpty(parts[3]) && !string.IsNullOrEmpty(parts[4]))
            {
                try
                {
                    double lat = double.Parse(parts[3]);
                    double degrees = Math.Floor(lat / 100);
                    double minutes = lat - (degrees * 100);
                    Latitude = degrees + (minutes / 60);
                    if (parts[4] == "S") Latitude *= -1;
                }
                catch { }
            }

            // Longitude
            if (!string.IsNullOrEmpty(parts[5]) && !string.IsNullOrEmpty(parts[6]))
            {
                try
                {
                    double lon = double.Parse(parts[5]);
                    double degrees = Math.Floor(lon / 100);
                    double minutes = lon - (degrees * 100);
                    Longitude = degrees + (minutes / 60);
                    if (parts[6] == "W") Longitude *= -1;
                }
                catch { }
            }

            // Speed (in knots)
            if (!string.IsNullOrEmpty(parts[7]))
            {
                try
                {
                    Speed = double.Parse(parts[7]);
                }
                catch { }
            }

            // Course (in degrees)
            if (!string.IsNullOrEmpty(parts[8]))
            {
                try
                {
                    Course = double.Parse(parts[8]);
                }
                catch { }
            }

            RaiseGpsDataReceivedEvent();
        }

        private void ProcessGpgsvSentence(string[] parts)
        {
            // $GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45*75
            if (parts.Length < 4) return;

            // Total satellites in view
            if (!string.IsNullOrEmpty(parts[3]))
            {
                try
                {
                    SatellitesInView = int.Parse(parts[3]);
                }
                catch { }
            }
        }

        private void RaiseGpsDataReceivedEvent()
        {
            var args = new GpsDataReceivedEventArgs
            {
                Latitude = Latitude,
                Longitude = Longitude,
                Altitude = Altitude,
                FixTime = FixTime,
                SatellitesInView = SatellitesInView,
                SatellitesInUse = SatellitesInUse,
                Speed = Speed,
                Course = Course,
                HasFix = HasFix
            };

            GpsDataReceived?.Invoke(this, args);
        }
    }

    /// <summary>
    /// Event arguments for GPS data received events
    /// </summary>
    public class GpsDataReceivedEventArgs : EventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public DateTime FixTime { get; set; }
        public int SatellitesInView { get; set; }
        public int SatellitesInUse { get; set; }
        public double Speed { get; set; } // in knots
        public double Course { get; set; } // in degrees
        public bool HasFix { get; set; }
    }
}