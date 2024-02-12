using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using Iot.Device.Ahtxx;


namespace websajt
{


    public class Program
    {

        static Aht20 sensor_temperature = null;
        static string telemetrydataFilePath = null;


        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            
            const string ssid = "xxxxxxx"; //SSID
            const string password = "xxxxxx"; //Heslo
            const string DirectoryPath = "I:\\";
            telemetrydataFilePath = DirectoryPath + "telemetryData.json";



            #region Config
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.IsHoldingEnabled = true;

            Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);



            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Aht20.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            sensor_temperature = new Aht20(i2cDevice);
            #endregion

            Wifi wifi = new Wifi(ssid, password, telemetrydataFilePath);
            Sensor sensor = new Sensor(sensor_temperature, telemetrydataFilePath);


            button.Press += (sender, e) => websajt.Sensor.ReadData();

            button.Holding += (sender, e) => websajt.Sensor.DeleteData();

            Timer pub_Timer = new Timer(websajt.Sensor.WriteData, null, 10000, 600000);


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }
}