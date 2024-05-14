using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.Networking;
using System.Net.NetworkInformation;
using System.IO;
using nanoFramework.Hardware.Esp32;



namespace WebServerTest
{
    public class Wifi
    {
        static string _ssid = null;
        static string _password = null;
        static string _telemetrydataFilePath = null;
        public Wifi(string ssid, string password, string telemetrydataFilePath)
        {
            _ssid = ssid;
            _password = password;
            _telemetrydataFilePath = telemetrydataFilePath;

            WIficonnect();

        }

        public static void WIficonnect()
        {
            try
            {
                CancellationTokenSource cs = new(90000);
                Debug.WriteLine($"Connecting to {_ssid} network ...");
                var success = WifiNetworkHelper.ScanAndConnectDhcp(_ssid, _password, requiresDateTime: true, token: cs.Token);
                if (success)
                {
                    //Blink(0, 10, 0, 100, 2);
                    Debug.WriteLine($"Wifi ready - {_ssid}, mac:{GetMacId(out string ipaddress)}, ip:{ipaddress}");
                    // Otherwise, you are connected and have a valid IP and date
                    Debug.WriteLine($" {WifiNetworkHelper.Status}");

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

            }
        }

        private static void ServerCommandReceived(object source, WebServerEventArgs e)
        {
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

            if (url.ToLower() == "/sayhello")
            {
                //Under construction
                string body = File.ReadAllText(_telemetrydataFilePath);
                WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html> <html><head>" +
"<title>Hi from nanoFramework Server</title></head><body> <br><p id=\"demo\"></p><p>" + body + "</p></body>" +
"<script>let text = " + body + ";\r\nconst myArray = text.split();\r\n\r\ndocument.getElementById(\"demo\").innerHTML = myArray;\r\n</script></html>");
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
    }
}