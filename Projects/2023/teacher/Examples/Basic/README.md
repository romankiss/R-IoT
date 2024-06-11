<!-- language-all: lang-csharp -->
<h2>Useful code for device connectivity, storage, etc.</h2>

based on the 
https://github.com/nanoframework/Samples/tree/main
   


<br></br>
<br></br>

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

<h3>Storage</h3>


       // E = USB storage
       // D = SD Card
       // I = Internal storage
       const string DirectoryPath = "I:\\";
       const string telemetrydataFilePath = DirectoryPath + "telemetryData.json";

  <h4>Show all files in the DirectoryPath</h4>     

       string[] listFiles = Directory.GetFiles(DirectoryPath);
       Debug.WriteLine("FileStorage:");
       foreach (var file in listFiles)
       {
           Debug.WriteLine($" {file}");
       }

   <h4>Show contents of the specific file, for example: telemetryFilePath</h4>

       string body = File.ReadAllText(telemetrydataFilePath);
       Debug.WriteLine($"{telemetrydataFilePath}:\r\n{body}");

or
       
       foreach (var file in listFiles)
       {
           if (file == telemetrydataFilePath)
           {
               Debug.WriteLine($"{file}:");
               using (FileStream fs = new FileStream(telemetrydataFilePath, FileMode.Open, FileAccess.Read))
               {
                   byte[] data = new byte[fs.Length];
                   fs.Read(data, 0, data.Length);
                   Debug.WriteLine($"{Encoding.UTF8.GetString(data, 0, data.Length)}");
               }
           }
       }

<h4>Append a telemetry data to internal storage</h4>

       var data = $"[{DateTime.UtcNow.ToString("hh:mm:ss.fff")}] temp={temp:F2}, hum={hum:F2}\r\n";
       using (FileStream fs = new FileStream(telemetrydataFilePath, FileMode.Append))
       {
           fs.Write(Encoding.UTF8.GetBytes(data), 0, data.Length);
           Debug.Write($"FileStorage.Write: {data}");
       }


 <h4>Delete file from Storage</h4>

        if(File.Exists(telemetrydataFilePath))
               File.Delete(telemetrydataFilePath);
        

<br></br>
<br></br>

<h3>Web Server</h3>

       #region WebServer
       using (WebServer server = new WebServer(80, HttpProtocol.Http)) //, new Type[] { typeof(ControllerTest) }))
       {
           server.CommandReceived += ServerCommandReceived;
           server.Start();
           Debug.WriteLine($"WebServer started.");
           Blink(0, 40, 0);
           Thread.Sleep(Timeout.Infinite);
        }
      
         static void ServerCommandReceived(object source, WebServerEventArgs e)
         {
             Blink();
             
             var url = e.Context.Request.RawUrl;
             Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");
             var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
         
             if (e.Context.Request.HttpMethod == HttpMethod.Get.Method)
             {
                 if (url.ToLower() == "/sayhello")
                 {
                     WebServer.OutPutStream(e.Context.Response, $"Hello from nanoFramework, available memory: {memory}");
                 }
                 else if (url.ToLower() == "/telemetrydata" && File.Exists(telemetrydataFilePath))
                 {
                     WebServer.SendFileOverHTTP(e.Context.Response, telemetrydataFilePath);
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
         #endregion

<h4>Using browser to get the data from the WebServer</h4>

![image](https://github.com/romankiss/R-IoT/assets/30365471/c84f72a0-1527-46a2-9072-a1d7e4c9a537)



![image](https://github.com/romankiss/R-IoT/assets/30365471/ff4874bd-50c5-43f9-985f-8189601247ca)


<br></br>
<br></br>

[Mini OLED Unit 0.42" 72x40 Display](https://shop.m5stack.com/products/mini-oled-unit-0-42-72x40-display)

NuGet Assembly: nanoFramework.Iot.Device.Ssd13xx

      #region OLED72x40
      I2cDevice i2c_oled72x40 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C, I2cBusSpeed.StandardMode));
      var res = i2c_oled72x40.WriteByte(0x07);
      if (res.Status == I2cTransferStatus.FullTransfer)
      {
          display = new Ssd1306(i2c_oled72x40) { Width = 99, Height = 64, Font = new Sinclair8x8(), Orientation = Iot.Device.Ssd13xx.DisplayOrientation.Landscape180 };
          display.ClearScreen();
      }
      #endregion




      var i2c_aht20 = AtomLite.GetGrove(Aht20.DefaultI2cAddress);
      res = i2c_aht20.WriteByte(0x07);
      if (res.Status == I2cTransferStatus.FullTransfer)
      {
          aht20 = new Aht20(i2c_aht20);  //seeed, aht20+BMP280,     = 0x38
          Debug.WriteLine($"Temp = {aht20.GetTemperature().DegreesCelsius} °C, Hum = {aht20.GetHumidity().Percent} %");
      }










       
