using System;
using System.Diagnostics;
using Iot.Device.Ssd13xx;
using Iot.Device.Adxl343Lib;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Numerics;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Iot.Device.Hcsr501;
using System.Device.Gpio;
using System.Device.Wifi;
using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Net.NetworkInformation;
using nanoFramework.WebServer;

namespace AccelDisplay
{
    public class Program
    {
        private static Sk6812 neo = null;
        private static string directoryPath = "I:\\";
        private static string telemetrydataFilePath = directoryPath + "telemetryData.json";
        private static string data = null;
        private static string ssid = "AS PIESTANY 3";
        private static string password = "123456789";
        private static WebServer server = new WebServer(80, HttpProtocol.Http);

        #region DeviceConfigs

        public static Ssd1306 displayConfig()
        {
            Configuration.SetPinFunction(25, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(21, DeviceFunction.I2C2_CLOCK);

            var i2cDisplay = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            Debug.WriteLine($"BUS SPEED DISPLAY: {i2cDisplay.ConnectionSettings.BusSpeed}");
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2cDisplay);
            display.Font = new Sinclair8x8();

            var overClock = new SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x0F);
            var adressingMode = new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Page);
            display.SendCommand(overClock);
            display.SendCommand(adressingMode);

            return display;
        }

        public static Adxl343 accelConfig()
        {
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C1_CLOCK);

            var i2cAccel = I2cDevice.Create(new I2cConnectionSettings(1, 0x53));
            Debug.WriteLine($"BUS SPEED ACCEL: {i2cAccel.ConnectionSettings.BusSpeed}");
            Adxl343 sensor = new Adxl343(i2cAccel, GravityRange.Range16);

            return sensor;
        }

        public static Hcsr501 motionConfig()
        {
            Hcsr501 motionSensor = new Hcsr501(23, PinNumberingScheme.Logical);
            return motionSensor;
        }



        #endregion




        #region Networking

        static void ConnectWifi(WifiAdapter sender, object e)
        {
            WifiNetworkReport report = sender.NetworkReport;

            foreach (WifiAvailableNetwork net in report.AvailableNetworks)
            {

                if (net.Ssid == ssid)
                {
                    sender.Disconnect();
                    WifiConnectionResult result = sender.Connect(net, WifiReconnectionKind.Automatic, password);


                    if (result.ConnectionStatus == WifiConnectionStatus.Success)
                    {
                        Debug.WriteLine("Connected to Wifi network");
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus.ToString()} connecting o Wifi network");
                    }

                }


            }

        }

        static string GetMacId(out string ipaddress)
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length > 0)
            {
                NetworkInterface ni = nis[0];
                ipaddress = ni.IPv4Address;
                return ByteArrayToHex(ni.PhysicalAddress);
            }
            else
            {
                ipaddress = string.Empty;
                return "000000000000";
            }
        }

        #endregion




        #region DataManipulation

        public static void ReadData()
        {
            string body = File.ReadAllText(telemetrydataFilePath);
            Debug.WriteLine($"{telemetrydataFilePath}:\r\n{body}");
            Thread.Sleep(100);

        }

        public static void WriteData(string data)
        {
            using (FileStream fs = new FileStream(telemetrydataFilePath, FileMode.Append))
            {
                fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);

            }
            Thread.Sleep(100);

        }

        public static void DeleteData()
        {
            Debug.WriteLine("Deleting storage");
            if (File.Exists(telemetrydataFilePath))
                File.Delete(telemetrydataFilePath);
            Thread.Sleep(100);

        }

        static string ByteArrayToHex(byte[] barray)
        {
            string bs = "";
            for (int i = 0; i < barray.Length; ++i)
            {
                bs += barray[i].ToString("X2");
            }
            return bs;
        }



        #endregion



        public static void Blink(byte r, byte g, byte b, int period, int count)
        {
            neo = new Sk6812(27, 3);

            for (int i = 0; i < count; i++)
            {
                neo.Image.SetPixel(0, 0, r, g, b);
                neo.Update();
                Thread.Sleep(period);
                neo.Image.SetPixel(0, 0, 0, 0, 0);
                neo.Update();
            }

        }
        /*
        static void ServerCommandReceived(object source, WebServerEventArgs e)
        {


            byte[] a = {1, 2};
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");
            var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);

            if (e.Context.Request.HttpMethod == HttpMethod.Get.Method)
            {
                if (url.ToLower() == "/sayhello")
                {
                    WebServer.OutPutStream(e.Context.Response, $"Hello from nanoFramework");
                }
                else if (url.ToLower() == "/telemetrydata" && File.Exists(telemetrydataFilePath))
                {
                    WebServer.SendFileOverHTTP(e.Context.Response, telemetrydataFilePath, a);
                }
                else
                {
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                }
            }
            else if (e.Context.Request.HttpMethod == HttpMethod.Delete.Method)
            {
                if (url.ToLower() == "/telemetrydata" && File.Exists(telemetrydataFilePath))
                {
                    File.Delete(telemetrydataFilePath);
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
                }
                else
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
            }
        }

        */

        public static void Main()
        {
            int movementCounter = 0;


            //Display
            Ssd1306 display = displayConfig();
            display.Orientation = DisplayOrientation.Landscape180;

            //Accel
            Adxl343 sensor = accelConfig();

            //Accel display
            Adxl343Display test1 = new Adxl343Display(256, 128, display, sensor);

            Adxl343Display test2 = new Adxl343Display(256, 128, display, sensor);

            //Motion sensor
            Hcsr501 motionSensor = motionConfig();

            //Button
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;

            Vector3 v = new Vector3();
            Vector3 vOld = new Vector3(0, 0, 0);

            display.ClearScreen();


            
            try
            {
                WifiAdapter wifi = WifiAdapter.FindAllAdapters()[0];
                wifi.AvailableNetworksChanged += ConnectWifi;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("message:" + ex.Message);
                Debug.WriteLine("stack:" + ex.StackTrace);
            }
            



            //server.CommandReceived += ServerCommandReceived;
            //server.Start();
            //Debug.WriteLine($"WebServer started.");
           


            if (!File.Exists(telemetrydataFilePath))
            {
                File.Create(telemetrydataFilePath);
                Thread.Sleep(100);
            }

            button.Press += (sender, e) =>
            {
                button.Holding += (sender, e) => DeleteData();

                string vString = v.ToString();
                string mcString = movementCounter.ToString();
                string data = "\n" + "Movement Counter: " + mcString + " Vector3: " + vString;
                WriteData(data);
            

                ReadData();

            };

            while (true)
            {
                if (sensor.TryGetAcceleration(ref v))
                {

                    if (Math.Abs(v.X - vOld.X) > 17 || Math.Abs(v.Y - vOld.Y) > 17)
                    {
                        //Display text

                        display.ClearScreen();

                        test1.WriteVector(64, 64, v, "Display is functional.");

                        test1.WriteVector(64, 48, v, "Movement counter:");

                        test1.WriteVector(64, 40, v, movementCounter.ToString());

                        display.Display();
                    }


                    if (motionSensor.IsMotionDetected)
                    {
                        //LED
                        
                        movementCounter++;
                        Blink(100, 0, 0, 1, 1);
                        Thread.Sleep(1);
                    }



                }



            }
           
        }




    }

}
