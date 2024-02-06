using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework;
using Iot.Device.Ssd13xx;
using System.Drawing;
using Iot.Device.Ws28xx.Esp32;
using System.Device.Gpio;
using Iot.Device.Button;
using Iot.Device.Sht3x;
using nanoFramework.AtomLite;
using Iot.Device.Ahtxx;
using UnitsNet;
//using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Device.Wifi;
using Iot.Device.Bh1750fvi;

namespace Display
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int i = 0;
        static Aht20 aht20 = null;
        static Bh1750fvi bh1750fvi = null;

        const string MYSSID = "AS PIESTANY 3";
        const string MYPASSWORD = "123456789";

        

        public static void Main()
        {
            GpioController gpio = new GpioController();
            neo = new Sk6812(27, 3);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)

            #region PINS
            //hub 
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(25, DeviceFunction.I2C1_DATA);
            
            #endregion
            
            // Create an instance of I2cDevice using the appropriate settings for your device
            //thermometer
            I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x38); // Replace with the correct I2C bus index and address
            I2cDevice i2cDevice = I2cDevice.Create(settings);


            //light sensore
            I2cConnectionSettings settings1 = new I2cConnectionSettings(1, 0x23);
            I2cDevice i2cdevice1 = I2cDevice.Create(settings1);




            #region WIFI
            //WIFI
            try
            {
                WifiAdapter wifi = WifiAdapter.FindAllAdapters()[0];
                wifi.AvailableNetworksChanged += Wifi_AvailableNetworksChanged;
                // give it some time to perform the initial "connect"
                // trying to scan while the device is still in the connect procedure will throw an exception
                Thread.Sleep(5000);

                
                int y = 0;
                while (y<1)
                {
                    y++;    
                    try
                    {
                        Debug.WriteLine("starting Wi-Fi scan");
                        wifi.ScanAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failure starting a scan operation: {ex}");
                    }

                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("message:" + ex.Message);
                Debug.WriteLine("stack:" + ex.StackTrace);
            }
            #endregion

            //====================================================//
            
            aht20 = new Aht20(i2cDevice);
            bh1750fvi = new Bh1750fvi(i2cdevice1);


            #region Temp&Hum  - sensor aht20  
            //AHT20 thermometer
            var i2c_aht20 = AtomLite.GetGrove(0x38);
            var res = i2c_aht20.WriteByte(0x07);
            //AH
            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                aht20 = new Aht20(i2c_aht20);  //seeed, aht20+BMP280,     = 0x38
                Debug.WriteLine($"Temp = {aht20.GetTemperature().DegreesCelsius} °C, Hum = {aht20.GetHumidity().Percent} %");
            }
            #endregion


            //button
            GpioButton button = new GpioButton(buttonPin: 39);
            button.Press += (sender, e) =>
            {

                Debug.WriteLine($"Press");

                UpdateDisplay();

            };
            Thread.Sleep(Timeout.Infinite);
        }

        //========================================//
        //need to fix:: light sensor, displaying on display; (correct initialization) 

        private static void UpdateDisplay()
        {
            
            // Control point
            if (aht20 == null || bh1750fvi == null)
            {
                Debug.WriteLine("Error!");
                return;
            }
            
            double temperature = aht20.GetTemperature().DegreesCelsius;
            double humidity = aht20.GetHumidity().Percent;
            //double illuminance = bh1750fvi.Illuminance.Lux;
            double illuminance = bh1750fvi.Illuminance.Value;


            I2cDevice i2cOled = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2cOled);
            display.ClearScreen();
            display.Font = new Sinclair8x8();

            DateTime currentTime = DateTime.UtcNow;
            string timeString = currentTime.ToString("HH:mm:ss");

            Debug.WriteLine("Current UTC time is: " + currentTime); //+1

            // Deploying things on display
            display.ClearScreen();

            display.DrawHorizontalLine(0, 0, 127);
            display.DrawHorizontalLine(0, 63, 127);
            display.DrawVerticalLine(0, 0, 63);
            display.DrawVerticalLine(127, 0, 63);

            //lux
            Debug.WriteLine($"LUX = {illuminance}");
            display.Write(5, 40, $"LUX = {illuminance}");
            //lux

            byte[] bitmap = { 0b111,
                              0b101,
                              0b111 };//°
            display.DrawBitmap(98, 21, 1, 3, bitmap);//°

            display.DrawBitmap(102, 20, 1, 8, display.Font['C']);


            //all important values
            display.DrawString(5, 20, $"Temp: {temperature:F2}");//°
            display.DrawString(5, 30, $"Humi: {humidity:F2} %");
            display.DrawString(5, 4, $"Time: {timeString}");//time
            
            display.DrawHorizontalLine(0, 15, 127);

            display.Display();

            Debug.WriteLine("----------");

        }
        
        private static void Wifi_AvailableNetworksChanged(WifiAdapter sender, object e)
        {
            Debug.WriteLine("Wifi_AvailableNetworksChanged - get report");

            // Get Report of all scanned Wifi networks
            WifiNetworkReport report = sender.NetworkReport;

            // Enumerate though networks looking for our network
            foreach (WifiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                // If its our Network then try to connect
                if (net.Ssid == MYSSID)
                {
                    // Disconnect in case we are already connected
                    sender.Disconnect();

                    // Connect to network
                    WifiConnectionResult result = sender.Connect(net, WifiReconnectionKind.Automatic, MYPASSWORD);
                }
            }
        }
        
    }
}

