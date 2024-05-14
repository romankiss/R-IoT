using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using Iot.Device.Sht3x;
using nanoFramework;
using System.Device.Gpio;
using WebServerTest;
//using Your.Mother;


namespace WebServerTest
{


    public class Program
    {

        static Sht3x sensorTH = null;
        static string telemetrydataFilePath = null;


        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            const string ssid = "Galaxy A35 5G 132C"; //SSID
            const string password = "123456789"; //Heslo
            const string DirectoryPath = "I:\\";
            telemetrydataFilePath = DirectoryPath + "telemetryData.json";



            #region Config
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;





            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(19, DeviceFunction.I2C1_DATA);
            sensorTH = new(new(new I2cConnectionSettings(1, 0x44)));





            #endregion


            Wifi wifi = new Wifi(ssid, password, telemetrydataFilePath);
           // Sensor sensor = new Sensor(sensorTH, telemetrydataFilePath);



            button.Press += (sender, e) => WebServerTest.Sensor.ReadData();

            button.Holding += (sender, e) => WebServerTest.Sensor.DeleteData();

            Timer pub_Timer = new Timer(WebServerTest.Sensor.WriteData, null, 10000, 10000);


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }
}