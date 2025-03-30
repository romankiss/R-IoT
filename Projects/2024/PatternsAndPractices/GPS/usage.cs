// usage of the GPS reader

using NFAppGPS;

static GPS sensorGPS = null;


#region GPS
    // GPS is event driven in comparison to other sensors 
    if (useGPS)
    {
        Configuration.SetPinFunction(pinCOM1_RX, DeviceFunction.COM1_RX);
        Configuration.SetPinFunction(pinCOM1_TX, DeviceFunction.COM1_TX);
        sensorGPS = GPS.Create("COM1", 115200);
        if (sensorGPS != null)
        {
            sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
            Debug.WriteLine($"GPS: {lat}, {lon}, {alt}");
        }
    }  
#endregion



private static void FireTelemetryData()
{
      ...
        
      //GPS
      bool isValidGPS = false;
      if (sensorGPS != null && sensorGPS.IsOpen)
      {
          // threadsafe parsing the GNGGA data received by serialport handler
          isValidGPS = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
          if(isvalidGPS)
          {
               payload += $"LA{lat}";    //add latitude to the payload
               payload += $"LO{lon}";    //add longitude to the payload
               payload += $"AL{alt}";    //add altitude to the payload
              
               // or
               // payload += $"L{lat},{lon}";    //add latitude and longitude to the payload
               // payload += $"A{alt}";          //add altitude to the payload
          }
      }

      ...  
}



