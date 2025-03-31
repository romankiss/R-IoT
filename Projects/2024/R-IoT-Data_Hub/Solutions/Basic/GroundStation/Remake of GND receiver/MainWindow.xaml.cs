using System;
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
        private readonly DatabaseManager _databaseManager;
        private readonly FileLogger _fileLogger;

        public const bool useDB = false;
        public const bool useFile = true;
        public const string fileHeader = "Counter,Temperature,Humidity,Pressure,Distance,Latitude,Longitude,Altitude\n";

        public MainWindow()
        {
            InitializeComponent();
            
            DataInterpretationComboBox.SelectedIndex = 1;//text
            BaudRateComboBox.SelectedIndex = 4;//115200
            ComPortComboBox.SelectedIndex = 0;//some port
            DisconnectButton.IsEnabled = false;

            _serialPortManager = new SerialPortManager();
            _telemetryProcessor = new TelemetryProcessor();
            if (useDB)_databaseManager = new DatabaseManager("Host=your-aws-endpoint;Username=your-user;Password=your-password;Database=your-db");
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

                string str;
                switch (_dataInterpretationMode) // Cache this value instead of reading UI
                {
                    case 0: // Hex
                        str = BitConverter.ToString(data);
                        break;
                    case 1: // Text
                            // Skip first 4 bytes (header) and last 2 bytes (footer/checksum)
                        int dataLength = data.Length - 6;
                        if (dataLength <= 0) return;
                        str = Encoding.ASCII.GetString(data, 3, dataLength-2);
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
                    if (useDB) await _databaseManager.SaveTelemetryAsync(telemetryData);
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

        public void SendButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            /*if (!_serialPortManager.IsConnected)
            {
                MessageTextBlock.Text = "Serial port not connected!";
                return;
            }
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _serialPortManager.SendAsync(data);*/
        }
        public async void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //await _fileLogger.LogToFileAsync(LogTextBox.Text);
        }
        public void ConnectToDBButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //await _databaseManager.ConnectAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            _serialPortManager.Dispose();
            base.OnClosed(e);
        }
    }
}
