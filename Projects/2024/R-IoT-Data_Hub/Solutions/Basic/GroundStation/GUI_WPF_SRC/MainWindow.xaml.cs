using System;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32; // For SaveFileDialog

namespace GUIforGND
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort; // SerialPort object for communication
        private DispatcherTimer _timer; // Timer for periodic updates

        public MainWindow()
        {
            InitializeComponent();
            RefreshComPorts(); // Populate COM ports when the window loads
            _serialPort = new SerialPort(); // Initialize SerialPort
            _timer = new DispatcherTimer(); // Initialize Timer
            _timer.Interval = TimeSpan.FromMilliseconds(100); // Set timer interval
            _timer.Tick += Timer_Tick; // Attach timer tick event

            // Set default baud rate selection
            BaudRateComboBox.SelectedIndex = 4; // Default to 115200
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
                _serialPort.BaudRate = selectedBaudRate; // Set selected baud rate
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.Open(); // Open the port

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

        // Timer tick event to read data from the COM port
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen && _serialPort.BytesToRead > 0) // Check if bytes are available
            {
                int bytesToRead = _serialPort.BytesToRead; // Get the number of bytes available
                byte[] buffer = new byte[bytesToRead]; // Create a buffer of the appropriate size
                _serialPort.Read(buffer, 0, bytesToRead); // Read all available bytes

                // Convert bytes to a readable format (e.g., hexadecimal)
                string data = BitConverter.ToString(buffer); // Convert bytes to hex string (e.g., "1A-2B-3C-4D-5E")

                // Append the data to the log with a timestamp
                Dispatcher.Invoke(() => LogTextBox.AppendText($"{DateTime.Now}: {data}\n"));
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

        // Cleanup when the window is closed
        protected override void OnClosed(EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close(); // Close the SerialPort
            }
            _timer.Stop(); // Stop the timer
            base.OnClosed(e);
        }
    }
}
