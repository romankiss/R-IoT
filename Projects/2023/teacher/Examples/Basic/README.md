<h2>Same usefull code for device connectivity, storage, etc.</h2>










<h3>WiFi Connection</h3>

       #region WiFi
       try
       {
           CancellationTokenSource cs = new(90000);
           Debug.WriteLine($"Connecting to {ssid} network ...");
           if (WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token))
           {
               //Blink(0, 10, 0, 100, 2);
               Debug.WriteLine($"Wifi ready - {ssid}, mac:{GetMacId(out string ipaddress)}, ip:{ipaddress}");
           }
           else
           {
               Debug.WriteLine($"Can't connect to the network {ssid}, error: {WifiNetworkHelper.Status}");
               throw new Exception($"Wifi is not connected.");
           }
       }
       catch (Exception ex)
       {
      
       }
       #endregion


<h3>Get IP and MAC addresses of the connected device </h3>

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





