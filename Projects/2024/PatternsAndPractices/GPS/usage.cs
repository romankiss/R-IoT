// usage of the GPS reader

using NFAppGPS;

static GPS sensorGPS = null;


#region GPS
Configuration.SetPinFunction(pinCOM1_RX, DeviceFunction.COM1_RX);
Configuration.SetPinFunction(pinCOM1_TX, DeviceFunction.COM1_TX);
sensorGPS = GPS.Create("COM1", 9600);
if (sensorGPS != null)
{
    bool isValid = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
    sensorGPS.OnGpsReceived_Unknown += (s, e) =>
    {
        //Debug.WriteLine(e.data);
    };
}
#endregion



private static void FireTelemetryData()
{
      ...
        
      //GPS
      bool isValidGPS = false;
      if (sensorGPS != null && sensorGPS.IsOpen)
      {
          isValidGPS = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
      }

      ...  
}



