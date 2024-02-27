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


namespace websajt
{
    public class Program
    {

        static string ssid = null;
        static string password = null;


        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            ssid = "realme 5";//"<YOUR_WIFI_NAME>";
            password = "qwertzui";//"<YOUR_WIFI_PASSWORD>";

            //Blink.set(0, 0, 15, 1000, 5);

            bool wifi_sucessful = WiFi.connect(ssid, password);
            if (wifi_sucessful)
            {
                Server.start();
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
