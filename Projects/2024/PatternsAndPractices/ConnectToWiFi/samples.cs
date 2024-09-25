
// user and password to wifi connect
static string ssid = "my ssid"; 
static string password = "abcd1234"; 
static string ssid2 = "my second ssid"; 
static string password2 = "01234567890"; 


// create a soft watchdog timer
Timer timer_watchdog = new Timer((s) => Power.RebootDevice(), null, 90000, 90000); // 1.5 minutes to finalize a device connectivity to ...


try
{
    #region Connect Wi-Fi
WiFi:
    CancellationTokenSource cs = new(60000);
    Debug.WriteLine($"Connecting to {ssid} network ...");
    Thread.Sleep(3000);
    if (WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token))
    {
        Debug.WriteLine($"Wifi ready - {ssid}, {GetMacId()}");
        // ...
    }
    else
    {
        Debug.WriteLine($"Can't connect to the network {ssid}, error: {WifiNetworkHelper.Status}");
        if (WifiNetworkHelper.HelperException != null)
        {
            Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
        }
        WifiNetworkHelper.Disconnect();
        if (ssid != ssid2)
        {
            ssid = ssid2;
            password = password2;
            Debug.WriteLine($"Trying to use a second connection choice for WiFi network {ssid}");
            WifiNetworkHelper.SetupNetworkHelper(ssid, password);
            goto WiFi;
        }
        throw new Exception($"Wifi is not connected.");
    }
    #endregion

    // Connect to MQTT, Http, ...
      
  }
  catch (Exception ex)
  {
      WifiNetworkHelper.Disconnect();
      var etext = ex.InnerException?.Message ?? ex.Message;
      Debug.WriteLine(etext);
      Thread.Sleep(3000);
      Power.RebootDevice();
  }
  finally
  {
      timer_watchdog.Dispose();
  }







