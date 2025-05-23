﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace Remake_of_GND_receiver
{
    public partial class MainWindow : Window
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly TelemetryProcessor _telemetryProcessor;
        private DatabaseManager _databaseManager;
        private readonly FileLogger _fileLogger;

        public const bool useDB = true;
        public const bool useFile = true;
        public const string fileHeader = "Timestamp,Counter,DevID,Temperature,Humidity,Distance,Pressure,Latitude,Longitude,Altitude,LaunchServoOpen,ParachutePin\n";

        public MainWindow()
        {
            InitializeComponent();
            
            DataInterpretationComboBox.SelectedIndex = 1;//text
            BaudRateComboBox.SelectedIndex = 4;//115200
            ComPortComboBox.SelectedIndex = 0;//some port
            DisconnectButton.IsEnabled = false;
            ConnectToDBButton.IsEnabled = useDB;

            _serialPortManager = new SerialPortManager();
            _telemetryProcessor = new TelemetryProcessor();
            //if (useDB)_databaseManager = new DatabaseManager("Host=your-aws-endpoint;Username=your-user;Password=your-password;Database=your-db");
            if(useFile)_fileLogger = new FileLogger(null,
                $"C:\\Users\\{Environment.UserName}\\Desktop\\sensor_data_{DateTime.Now:dd.MM.yyyy_HH-mm}.csv",
                fileHeader
            );

            RefreshButton_Click(null, null);
            _serialPortManager.DataReceived += OnSerialDataReceived;
            _serialPortManager.ErrorOccurred += OnSerialError;
        }

       private async void OnSerialDataReceived(object sender, byte[] data)
        {
            try
            {
                int _dataInterpretationMode = -1;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _dataInterpretationMode = DataInterpretationComboBox.SelectedIndex;
                });
                // Skip if data is too short (e.g., missing header/footer)
                if (data.Length < 6) return; // Adjust minimum length as needed

                string str = String.Empty;
                switch (_dataInterpretationMode) // Cache this value instead of reading UI
                {
                    case 0: // Hex
                        str = BitConverter.ToString(data);
                        break;
                    case 1: // Text
                            // Skip first 4 bytes (header) and last 2 bytes (footer/checksum)
                        int dataLength = data.Length;
                        if (dataLength <= 0) return;
                        str = Encoding.ASCII.GetString(data, 3, dataLength-4);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported data mode: {_dataInterpretationMode}");
                }

                // Update UI
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LogTextBox.AppendText($"{DateTime.Now}: {str}\n");
                    LogTextBox.ScrollToEnd();
                });

                // Parse and save telemetry
                var telemetryData = _telemetryProcessor.ParseData(str);
                if (telemetryData.Count > 0)
                {
                    if (useFile) await _fileLogger.LogToCsvAsync(telemetryData);
                    if (useDB && _databaseManager!=null) await _databaseManager.SaveTelemetryAsync(telemetryData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing data: {ex.Message}");
            }
        }
        private void OnSerialError(object sender, string errorMessage)
        {
            Dispatcher.Invoke(() => MessageTextBlock.Text = errorMessage);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _serialPortManager.ConnectAsync(ComPortComboBox.SelectedItem.ToString(), int.Parse(BaudRateComboBox.Text));
                MessageTextBlock.Text = "Connected successfully!";
                StartButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }
        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ComPortComboBox.Items.Clear();
            foreach (var portName in _serialPortManager.GetPortNames())
            {
                ComPortComboBox.Items.Add(portName);
            }
        }
        public void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _serialPortManager.Disconnect();
            MessageTextBlock.Text = "Disconnected!";
            StartButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }

        public async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                MessageTextBlock.Text = "Serial port not connected!";
                return;
            }

            try
            {
                var text = InputTextBox.Text;
                var data = Encoding.ASCII.GetBytes(text);
                await _serialPortManager.SendAsync(data);
                MessageTextBlock.Text = "Message sent successfully";
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Send failed: {ex.Message}";
            }
        }
        public async void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //await _fileLogger.LogToFileAsync(LogTextBox.Text);
        }
        public void ConnectToDBButton_Click(object sender, RoutedEventArgs e)
        {
            // throw new NotImplementedException();
            //await _databaseManager.ConnectAsync();
            /* if(ConnectionStringTextBox.Text == null)
            {
                MessageTextBlock.Text = "Connection string is empty!";
                return;
            }
            _databaseManager = new DatabaseManager(ConnectionStringTextBox.Text);*/
            ConnStrGatherer gatherer = null;
            if (ConnectionStringFilePathTextBox.Text.StartsWith("C:"))
            {
                gatherer = new ConnStrGatherer(ConnectionStringFilePathTextBox.Text); // For custom path
            }
            else
            {
                gatherer = new ConnStrGatherer(); // Uses default path
            }
                
                                                  // 
            string connectionString = gatherer.GetConnectionString();
            _databaseManager = new DatabaseManager(connectionString);
        }

        protected override void OnClosed(EventArgs e)
        {
            _serialPortManager.Dispose();
            base.OnClosed(e);
        }
    }
}
