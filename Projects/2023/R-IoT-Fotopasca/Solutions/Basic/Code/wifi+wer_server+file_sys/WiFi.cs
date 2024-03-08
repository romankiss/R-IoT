using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;


//using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace websajt
{
    internal class WiFi
    {
        public static bool connect(string ssid, string password)
        {


            #region WiFi

            CancellationTokenSource cs = new(60000);
            Debug.WriteLine($"Connecting to {ssid} network ...");
            if (WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token))
            {
                Debug.WriteLine($"Wifi ready - {ssid}, mac:{GetMacId(out string ipaddress)}, ip:{ipaddress}");

                return true;
            }
            else
            {
                Debug.WriteLine($"Can't connect to the network {ssid}, error: {WifiNetworkHelper.Status}");

                return false;
            }


            #endregion

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
}
