using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using GPStest;

namespace GPStest
{
    public class Program
    {

        static GPS sensorGPS = null;
        public static int pinCOM1_RX = 5;
        public static int pinCOM1_TX = 6;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");


            


            #region GPS
            Configuration.SetPinFunction(pinCOM1_RX, DeviceFunction.COM1_RX);
            Configuration.SetPinFunction(pinCOM1_TX, DeviceFunction.COM1_TX);
            sensorGPS = GPS.Create("COM1", 115200);
            if (sensorGPS != null)
            {
                bool isValid = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
                sensorGPS.OnGpsReceived_GNGGA += (s, e) =>
                {
                    Debug.WriteLine(e.data);
                };
            }
            #endregion


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
