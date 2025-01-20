using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.Networking;
using System.Net.NetworkInformation;
using System.IO;
using System.Globalization;
using LoRa_Web_Lamp.WEB;
using LoRa_Web_Lamp.WEB.Controllers;

namespace LoRa_Web_Lamp
{
    public class Wifi
    {
        static string _ssid = null;
        static string _password = null;
        private static int Num = 0;
        private static WebService _webService = null;

        public Wifi(string ssid, string password)
        {
            _ssid = ssid;
            _password = password;

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
                    Debug.WriteLine($"Wifi ready - {_ssid}, mac:{GetMacId(out string ipaddress)}, ip:{ipaddress}");
                    // Otherwise, you are connected and have a valid IP and date
                    Debug.WriteLine($" {WifiNetworkHelper.Status}");
                    ControllerPages ipa = new ControllerPages(ipaddress);

                    _webService = new WebService();
                }
                else
                {
                    if (_webService is not null)
                    {
                        _webService.Dispose();
                        _webService = null;
                    }
                    Debug.WriteLine($"Can't connect to the network {_ssid}, error: {WifiNetworkHelper.Status}");
                    throw new Exception($"Wifi is not connected.");
                }
            }
            catch (Exception ex)
            {

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
