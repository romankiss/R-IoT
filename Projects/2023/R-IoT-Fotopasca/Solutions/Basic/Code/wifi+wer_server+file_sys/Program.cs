using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using nanoFramework.WebServer;
using System.Device.Wifi;
using nanoFramework.Networking;
using nanoFramework.AtomLite;
using System.Net.NetworkInformation;
using System.IO;
using static websajt.Blink;
using static websajt.WiFi;
using static websajt.Server;
using Iot.Device.Button;


namespace websajt
{
    public class Program
    {

        static string ssid = null;
        static string password = null;
        static string file_path = null;


        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            ssid = "<YOUR_WIFI_NAME>";  
            password = "<YOUR_WIFI_PASSWORD>";


            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(100));
            button.IsHoldingEnabled = true;
            button.IsDoublePressEnabled = true;

            file_path = "I:\\telemetry_data.json";
            Storage.init(file_path);

            //Blink.set(0, 0, 15, 1000, 5);

            bool wifi_sucessful = WiFi.connect(ssid, password);
            if (wifi_sucessful)
            {
                Server.start(file_path);
            }


            button.Press += (sender, e) =>
            {
                websajt.Storage.new_formated_record(file_path, websajt.Sensor.measure().ToString());//after a button press make a new record 
                Debug.WriteLine($"Actual content: \n{websajt.Storage.read(file_path)}");
                Blink.set(10, 10, 10, 1, 0.5, 1);
            };

            button.Holding += (sender, e) =>
            {
                websajt.Storage.delete(file_path);//after a long hold delete the records
                Debug.WriteLine($"Deleting {file_path}");
                Blink.set(10, 0, 10, 1, 1, 1);
            };
            button.DoublePress += (sender, e) =>
            {
                Storage.list_files("I:\\");
                Blink.set(10, 10, 0, 1, 0.5, 5);
            };

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
