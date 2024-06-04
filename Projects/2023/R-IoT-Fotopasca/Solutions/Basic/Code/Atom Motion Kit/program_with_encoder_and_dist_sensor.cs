using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;//this library needs to be added
//using HT16K33;
using Iot.Device.Button;
using Iot.Device.Vl53L0X;


namespace NFAppAtomLite_Testing
{
    public class Program
    {
        public static int servo_angle = 25;
        public static M5AtomicMotion motionbase;
        private static Vl53L0X sensorToF;
        private static object res;

        // public static SimpleHT16K33 ht16k33 = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(38, DeviceFunction.I2C2_DATA);//38     onboard:2
                Configuration.SetPinFunction(39, DeviceFunction.I2C2_CLOCK);//39    onbrd:1
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }



            //Create the button object
            GpioButton button = new GpioButton(buttonPin: 41, debounceTime: TimeSpan.FromMilliseconds(200));


            // Initialize I2C device on the default address of 0x38(Motion Base) , display ht16k33(0x70)
            I2cDevice i2c = new I2cDevice(new I2cConnectionSettings(2, M5AtomicMotion.DefaultI2cAddress));
            motionbase = M5AtomicMotion.Create(i2c);
           
            //ht16k33 = SimpleHT16K33.Create(1, BackpackWiring.Default);



            //set the angle of the servo on the 4th (0,1,2,3) channel/port to 25degrees
           // motionbase.SetServoPulse(0, 1500);
            Debug.WriteLine($"angle: {servo_angle}");

            //bind the handler with the event watcher of button
            button.Press += (sender, e) => handle_button();


            #region ToF 
            I2cDevice i2c_tof = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
           
            
            sensorToF = new Vl53L0X(i2c_tof, 1000);
            if (sensorToF != null)
            {
                    sensorToF.HighResolution = true;
                    sensorToF.Precision = Precision.ShortRange;
                    sensorToF.MeasurementMode = MeasurementMode.Continuous;
                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                    
            }
            
            #endregion

            #region Encoder
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);     //AtomicMotionBase SD-G19, G33
            Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);    //AtomicMotionBase SC-G22, G23
            Encoder sensorEncoder = Encoder.Create(busId: 1, pollingTimeInMilliseconds: 50);
            if (sensorEncoder != null)
            {
                sensorEncoder.OnChangeValue += (sender, e) =>
                {
                    Encoder encoder = sender as Encoder;
                    if (e.OldValue != e.NewValue || e.ButtonStatus)
                    {
                        Debug.WriteLine($"sensorEncoder: Val={e.NewValue}, button={e.ButtonStatus}");
                        if (e.NewValue < M5AtomicMotion.ServoAngleBack)
                            encoder.SetLEDColor(0, (byte)(e.NewValue), 0, 0);
                        else if (e.NewValue > M5AtomicMotion.ServoAngleAhead)
                            encoder.SetLEDColor(0, 0, 0, (byte)(e.NewValue));
                        else
                        {
                            encoder.SetLEDColor(0, 0, (byte)(e.NewValue), 0);
                            if (motionbase != null && e.ButtonStatus)
                            {
                                Debug.WriteLine($"ServoAngle motion = {e.NewValue}");
                                motionbase.SetServoAngle(0, (byte)(e.NewValue));
                                if (sensorToF != null)
                                {
                                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                                }
                            }
                        }
                    }
                };
            }
            #endregion





            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        public static void handle_button()
        {


            Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
            /* //get the current angle of the servo
             servo_angle = servo.GetServoAngle(3);
             if (servo_angle >= 85)
             {
                 //if the clamp is already closed, open it again
                 servo_angle = 25;
             }
             else
             {
                 //if the clamp is not fully closed yet close it a bit
                 servo_angle += 5;
             }
             servo.SetServoAngle(3, (byte)servo_angle);
             Debug.WriteLine($"angle: {servo_angle}");

             */

            /*if (ht16k33 != null)
            {
                ht16k33.Init();
                ht16k33.SetBrightness(10);
                ht16k33.SetBlinkRate();
                ht16k33.Clear();

                ht16k33.ShowMessage("SPSE Piestany", true, 100);
                //ht16k33.ShowMatrix(0x183c7effffff6600, false, 100);
                //ht16k33.ShowAndCirculateMessageAsync("", 250);
                //ht16k33.ShowArray(buffer, bScrollLastCharacters: false);

            }
            else
            {
                Debug.WriteLine("================");
            }**/
        }
    }
}
/*

// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.

using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace WebServerE2ETests
{
    public class Program
    {
        private const string Ssid = "realme 5";
        private const string Password = "123456789";//"3mgdrr5kt3ph7nun";
        private static WebServer _server;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework WebServer end to end tests!");

            var res = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
            if (!res)
            {
                Debug.WriteLine("Impossible to connect to wifi, most likely invalid credentials");
                return;
            }

            Debug.WriteLine($"Connected with wifi credentials. IP Address: {GetCurrentIPAddress()}");
            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(SimpleRouteController), typeof(AuthController) });
            // To test authentication with various scenarios
            _server.ApiKey = "ATopSecretAPIKey1234";
            _server.Credential = new NetworkCredential("topuser", "topPassword");
            // Add a handler for commands that are received by the server.
            _server.CommandReceived += ServerCommandReceived;
            _server.WebServerStatusChanged += WebServerStatusChanged;

            // Start the server.
            _server.Start();

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void WebServerStatusChanged(object obj, WebServerStatusEventArgs e)
        {
            Debug.WriteLine($"The web server is now {(e.Status == WebServerStatus.Running ? "running" : "stopped")}");
        }

        private static void ServerCommandReceived(object obj, WebServerEventArgs e)
        {
            const string FileName = "I:\\Text.txt";
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"{nameof(ServerCommandReceived)} {e.Context.Request.HttpMethod} {url}");

            if (url.ToLower().IndexOf("/param.htm") == 0)
            {
                // Test with parameters
                var parameters = WebServer.DecodeParam(url);
                string toOutput = "<html><head>" +
                    "<title>Hi from nanoFramework Server</title></head><body>Here are the parameters of this URL: <br />";
                foreach (var par in parameters)
                {
                    toOutput += $"Parameter name: {par.Name}, Value: {par.Value}<br />";
                }
                toOutput += "</body></html>";
                WebServer.OutPutStream(e.Context.Response, toOutput);
                return;
            }
            else if (url.IndexOf("/Text.txt") == 0)
            {
                if (File.Exists(FileName))
                {
                    byte[] data = File.ReadAllBytes(FileName);
                    WebServer.SendFileOverHTTP(e.Context.Response, FileName, data, "txt");
                    return;
                }
                else
                {
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                    return;
                }
            }
            else if (url.IndexOf("/Text2.txt") == 0)
            {
                WebServer.SendFileOverHTTP(e.Context.Response, "Text2.txt", Encoding.UTF8.GetBytes("This is a test file for WebServer"));
                return;
            }
            else if (url.ToLower().IndexOf("/useinternal") == 0)
            {
                File.WriteAllText(FileName, "This is a test file for WebServer");
                return;
            }
            else
            {
                WebServer.OutPutStream(e.Context.Response, "<html><head>" +
                    "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
                    "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
                    "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
                return;
            }
        }

        public static string GetCurrentIPAddress()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            // get first NI ( Wifi on ESP32 )
            return ni.IPv4Address.ToString();
        }
    }
}*/
