using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.Networking;
using nanoFramework.AtomLite;
using System.Net.NetworkInformation;
using System.IO;
using Iot.Device.Ws28xx.Esp32;



namespace Wifii
{
    public class Wifi
    {
        static string _ssid = null;
        static string _password = null;
        static string _telemetrydataFilePath = null;
        static Sk6812 neo = null;

        public Wifi(string MYSSID, string MYPASSWORD, string telemetrydataFilePath)
        {
            _ssid = MYSSID;
            _password = MYPASSWORD;
            _telemetrydataFilePath = telemetrydataFilePath;

            neo = new Sk6812(27, 3);
            neo = AtomLite.NeoPixel;

            WIficonnect();

        }

        public static void WIficonnect()
        {
            try
            {
                CancellationTokenSource cs = new(90000);
                Debug.WriteLine($"Connecting to {_ssid} network ...");
                if (WifiNetworkHelper.ScanAndConnectDhcp(_ssid, _password, requiresDateTime: true, token: cs.Token))
                {
                    //Blink(0, 10, 0, 100, 2);
                    Debug.WriteLine($"Wifi ready - {_ssid}, mac:{GetMacId(out string ipaddress)}, ip:{ipaddress}");
                    // Otherwise, you are connected and have a valid IP and date
                    Debug.WriteLine($" {WifiNetworkHelper.Status}");

                    Blinks(0, 0, 15);

                    WebServer server = new WebServer(80, HttpProtocol.Http);

                    // Add a handler for commands that are received by the server.
                    server.CommandReceived += ServerCommandReceived;

                    // Start the server.
                    server.Start();
                }
                else
                {
                    Debug.WriteLine($"Can't connect to the network {_ssid}, error: {WifiNetworkHelper.Status}");
                    throw new Exception($"Wifi is not connected.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception");
            }
        }

        private static void ServerCommandReceived(object source, WebServerEventArgs e)
        {
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

            if (url.ToLower() == "/sayhello")
            {
                string body = File.ReadAllText(_telemetrydataFilePath);
                
                WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html> <html><head>" +
"<title>Hi from nanoFramework Server</title></head><body> <br> " + body + "</body></html>");
            }
            else
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
            }
        }

        #region GetMacId
        static string GetMacId(out string ipaddress)
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length > 0)
            {
                // get the first interface
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

        #region Blink
        public static void Blinks(byte r, byte g, byte b, int periodIntMs = 100, int times = 1)
        {
            for (int time = times; time > 0; --time)
            {
                neo.Image.SetPixel(0, 0, r, g, b);
                neo.Update();
                Thread.Sleep(periodIntMs);
                neo.Image.Clear();
                neo.Update();
                if (time > 1) Thread.Sleep(periodIntMs);
            }

        }
        #endregion
    }
}
