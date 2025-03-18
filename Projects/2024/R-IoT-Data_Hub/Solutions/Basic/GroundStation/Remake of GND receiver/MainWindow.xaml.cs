using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System.Globalization;
using Npgsql;
using System.Text.RegularExpressions;


namespace GUIforGND
{
    public partial class MainWindow : Window
    {
        public class SensorData
        {
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public int Pressure { get; set; }
            public int Distance { get; set; }
            public int Counter { get; set; }
        }

        public SensorData sensordata { get; set; } = new SensorData();
       
        private static string csvFilePath = "C:\\Users\\"+ Environment.UserName+"\\OneDrive\\Desktop\\sensor_data_" + DateTime.Now.ToString("dd. MM. yyyy HH-mm") + ".csv";
        private SerialPort _serialPort; // SerialPort object for communication
        private DispatcherTimer _timer; // Timer for periodic updates
        readonly object _sendLock = new(); // For thread-safe writing to the device
        public static Dictionary<string, string> telemetryData { get; set; } = new Dictionary<string, string>();

        // Connection string for your AWS PostgreSQL database
        string connectionString = "Host=your-aws-endpoint;Username=your-user;Password=your-password;Database=your-db";
        // SQL query to insert data
        string query = @"
            INSERT INTO sensor_data (temperature, humidity, pressure, distance, counter)
            VALUES (@temperature, @humidity, @pressure, @distance, @counter)";

        public MainWindow()
        {
            InitializeComponent();


            // Initialize serial port and timer
            _serialPort = new SerialPort();
            RefreshComPorts();
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += Timer_Tick;

            // Set default selections
            BaudRateComboBox.SelectedIndex = 4; // Default to 115200
            DataInterpretationComboBox.SelectedIndex = 1; // Default to text

            // Check if the file exists, if not, create it and write the header
            if (!File.Exists(csvFilePath))
            {
                using (StreamWriter sw = File.CreateText(csvFilePath))
                {
                    sw.WriteLine("Counter, Temperature, Humidity, Distance, Pressure");
                }
            }


        }

        // Method to refresh the list of available COM ports
        private void RefreshComPorts()
        {
            ComPortComboBox.Items.Clear();
            string[] comPorts = SerialPort.GetPortNames();
            foreach (string port in comPorts)
            {
                ComPortComboBox.Items.Add(port);
            }
            if (ComPortComboBox.Items.Count > 0)
            {
                ComPortComboBox.SelectedIndex = 0;
            }
        }

        // Event handler for the Refresh button
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshComPorts();
        }

        // Event handler for the Start Reading button
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComPortComboBox.SelectedItem == null)
            {
                MessageTextBlock.Text = "Please select a COM port!";
                return;
            }

            if (BaudRateComboBox.SelectedItem == null)
            {
                MessageTextBlock.Text = "Please select a baud rate!";
                return;
            }

            string selectedPort = ComPortComboBox.SelectedItem.ToString();
            int selectedBaudRate = int.Parse((BaudRateComboBox.SelectedItem as ComboBoxItem).Content.ToString());

            try
            {
                // Configure the SerialPort
                _serialPort.PortName = selectedPort;
                _serialPort.BaudRate = selectedBaudRate;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.Open();

                // Start the timer to read data
                _timer.Start();
                MessageTextBlock.Text = $"Reading from {selectedPort} at {selectedBaudRate} baud...";

                // Enable/disable buttons
                StartButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }
        // Event handler for the Save Log button
        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open a SaveFileDialog to choose the file location
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = "Log.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Write the log content to the selected file
                    File.WriteAllText(saveFileDialog.FileName, LogTextBox.Text);
                    MessageTextBlock.Text = $"Log saved to {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error saving log: {ex.Message}";
            }
        }

        // Event handler for the Send button
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPort.IsOpen)
            {
                MessageTextBlock.Text = "COM port is not open!";
                return;
            }

            string input = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageTextBlock.Text = "Please enter data to send in a format specified in the selection!";
                return;
            }

            try
            {
                if (DataInterpretationComboBox.SelectedIndex == 0)//hex
                {
                    // Convert the input string to a byte array

                    SendDataAndWrappWithMeta(data: ParseHexString(input));
                    MessageTextBlock.Text = $"Sent: {input}";
                }
                else if (DataInterpretationComboBox.SelectedIndex == 1)//text
                {
                    SendDataAndWrappWithMeta(data: input);
                    MessageTextBlock.Text = $"Sent: {input}";
                }
                else
                {
                    MessageTextBlock.Text = "Invalid format of data interpretation!!!";
                    return;
                }
                // Send the byte array to the device
                //_serialPort.Write(data, 0, data.Length); 
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error sending data: {ex.Message}";
            }
        }

        // Helper method to convert a hex string to a byte array
        private byte[] ParseHexString(string hex)
        {
            hex = hex.Replace(" ", ""); // Remove spaces
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        //took from the e22 wrapper written by R.K. - prev. named as Send
        //takes a bytearray as data and uses also params. like receivers and our own modem address, channel etc. and wrapps the data, adds a chescksum, which is computed using the Extensions.cs and writes an array of bytes to the modem that he understands
        public bool SendDataAndWrappWithMeta(ushort address = 0xFFFF, byte[] data = null, byte channel = 0x12)
        {
            byte[] header = new byte[7];

            try
            {
                //to prevent overflowing the around 240B big packet limit with a tolerance
                if (_serialPort == null || !_serialPort.IsOpen) return false;
                if (data != null && data.Length > 197) return false;
                byte datalength = (byte)(data == null ? 0 : data.Length);
                if (header.Length + datalength > 200) return false;

                //building the Byte arr.
                header[0] = (byte)((address >> 8) & 0xFF);
                header[1] = (byte)(address & 0xFF);
                header[2] = channel;    // channel
                header[3] = 0xC1;       // start mark
                header[4] = (byte)((0x00 >> 8) & 0xFF);//ownADD
                header[5] = (byte)(0x0A & 0xFF);//ownADD
                header[6] = datalength;

                byte[] bytes = new byte[header.Length + datalength + 1];
                header.CopyTo(bytes, 0);
                data?.CopyTo(bytes, header.Length);
                //bytes[header.Length + datalength] = 0x00; // End of Data
                bytes[header.Length + datalength] = bytes.ComputeChecksum(4, (uint)datalength + 3);  //ADDH+ADDL+LEN

                //actualy sending to modem
                lock (_sendLock)
                {
                    Debug.WriteLine($"LoRaSend[{bytes.Length}]: {BitConverter.ToString(bytes)}");
                    _serialPort.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEP when LoRaSend: {ex.Message}");
                return false;
            }
        }

        public bool SendDataAndWrappWithMeta(ushort address = 0xFFFF, string data = "Ping", byte channel = 0x12)
        {
            return SendDataAndWrappWithMeta(address, Encoding.UTF8.GetBytes(data ??= ""), channel);
        }

        // Event handler for the Disconnect button
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Stop the timer and close the port
                _timer.Stop();
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                MessageTextBlock.Text = "Disconnected.";

                // Enable/disable buttons
                StartButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        public void ConnectToDBButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectionString = ConnectionStringTextBox.Text;
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                MessageTextBlock.Text = "Connected to the database!";
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error connecting to the database: {ex.Message}";
                Debug.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }

        // Timer tick event to read data from the COM port
        private void Timer_Tick(object sender, EventArgs e)
        {

            if (_serialPort.IsOpen && _serialPort.BytesToRead > 0)
            {
                int bytesToRead = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                _serialPort.Read(buffer, 0, bytesToRead);

                string data = "";
             

                if (DataInterpretationComboBox.SelectedIndex == 0)
                {
                    // Convert bytes to a readable format (e.g., hexadecimal)
                    data = BitConverter.ToString(buffer); // Convert bytes to hex string (e.g., "1A-2B-3C-4D-5E")
                }
                else if (DataInterpretationComboBox.SelectedIndex == 1)
                {
                    //Extract only the data Bytes and convert them to chars to make a text
                    StringBuilder sb = new StringBuilder();
                    //< 5th. (index = 4) B; (bytesToRead-1)th. (index = bytesToRead-2) ) is the interval whare the data is
                    for (int i = 4; i < (bytesToRead - 2); i++)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    data = sb.ToString();



                    // extracting a telemetry data
                    // sample
                    //string input = "#123T22.33H10.05P0.12345D-1\r\n";
                    // creating a telemetryData dictionary
                    try
                    {
                        var values = Regex.Split(data, @"([#\r\nA-Z]+)").Where(s => s != String.Empty && s != "\r\n").ToArray();
                        telemetryData = Enumerable.Range(0, values.Length / 2).ToDictionary(i => values[2 * i], i => values[2 * i + 1]);
                        // usage
                        bool isTemperatureExtracted = float.TryParse(telemetryData["T"], out float temperature);
                        bool isHumidityExtracted = float.TryParse(telemetryData["H"], out float humidity);
                        bool isPressureExtracted = int.TryParse(telemetryData["P"], out int pressure);
                        bool isDistanceExtracted = int.TryParse(telemetryData["D"], out int distance);
                        bool isCounterExtracted = int.TryParse(telemetryData["#"], out int counter);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine("Error parsing the value from the data packet: " + ex.Message);
                    }



                    // Update the log
                    // Parse the data (e.g., "T25.12H48.12P998")
                    //also note, that i had to get away the \r\n from the end of the data, because the data was not parsed correctly, (removed from the sender side)
                   /* sensordata.Temperature = ExtractValue(data, 'T');
                    sensordata.Humidity = ExtractValue(data, 'H');
                    sensordata.Pressure = ExtractValue(data, 'P');
                    sensordata.Distance = ExtractValue(data, 'D');
                    sensordata.Counter = ExtractValue(data, '#');*/

                    WriteDataToCsv();

                }
                else
                {
                    MessageTextBlock.Text = "Invalid format of data interpretation!!!";
                    return;
                }



                /*Dispatcher.Invoke(() => LogTextBox.AppendText($"{DateTime.Now}: {$"Temperature: {temperature}, Humidity: {humidity}, Pressure: {pressure}, Distance: {distance}"}\n"));
                LogTextBox.ScrollToEnd();*/

                // Update the log
                Dispatcher.Invoke(() => LogTextBox.AppendText($"{DateTime.Now}: {data}\n"));
                LogTextBox.ScrollToEnd();

            }

        }


        private static void WriteDataToCsv()
        {
            using (StreamWriter sw = File.AppendText(csvFilePath))
            {
                // simple csv record
                string csv = string.Join(",", telemetryData.Values) + "\r\n";
                sw.Write(csv);
                //sw.WriteLine($"\"{data.Temperature}\",\"{data.Humidity}\",\"{data.Pressure}\",\"{data.Distance}\",{data.Counter}"); //vals need to be stored in prentecies ("), because the parsing saves them with a comma (,) and the csv would interpret it as a new column
            }
        }

        // Helper method to extract a value after a specific letter
        private double ExtractValue(string data, char prefix)
        {
            int startIndex = data.IndexOf(prefix);
            if (startIndex == -1)
            {
                Debug.WriteLine("Searched var identifier not found in data packet...");
                return -1;
            }//throw new ArgumentException($"Prefix '{prefix}' not found in data.");

            int endIndex = startIndex + 1;
            while (endIndex < data.Length && (char.IsDigit(data[endIndex]) || data[endIndex] == '.'))
            {
                endIndex++;
            }
            try
            {
                string valueString = data.Substring(startIndex + 1, endIndex - startIndex - 1);
                return double.Parse(valueString, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error parsing the value from the data packet: " + ex.Message);
                return -1;

            }
        }

        // Cleanup when the window is closed
        protected override void OnClosed(EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _timer.Stop();
            
            base.OnClosed(e);
        }
    }
}