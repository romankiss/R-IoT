using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Sht4x;

namespace ENVsnsr
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(8 , DeviceFunction.I2C2_DATA);//this line tells, that the GPIO pin no. 8 will be used as the data pin for the second i2c bus/interface(?)
                Configuration.SetPinFunction(7, DeviceFunction.I2C2_CLOCK);//the 7th pin will be the carrier of the CLK signal ...
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }

            /*I2cDevice temp_hum_meter = I2cDevice.Create(new I2cConnectionSettings(2, 0x44, I2cBusSpeed.StandardMode));

            I2cDevice pressure_meter = I2cDevice.Create(new I2cConnectionSettings(2, 0x76, I2cBusSpeed.StandardMode));*/

            I2cConnectionSettings I2C_settings = new(2, Sht4X.I2cDefaultAddress);
            using I2cDevice temp_hum_meter_device = I2cDevice.Create(I2C_settings);//i2c dev creation
            using Sht4X temp_hum_meter_snsr = new(temp_hum_meter_device);//encapsulationg the i2c dev into the sht40 class which includes methods for easy communication


            temp_hum_meter_device.WriteByte(0x96);//send soft reset to avoid initial CRC non-validity
            while (true)
            {
                try
                {
                    var data = temp_hum_meter_snsr.ReadData(MeasurementMode.NoHeaterHighPrecision);

                    Debug.WriteLine($"Temperature: {data.Temperature.DegreesCelsius}\u00B0C");
                    Debug.WriteLine($"Relative humidity: {data.RelativeHumidity.Percent}%RH");
                }
                catch (Exception ex) { Debug.WriteLine("exeption: " + ex); }
  
          
                Thread.Sleep(1000);
            }
            
     

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
