using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Button;
using nanoFramework.Json;
using System.Device.I2c;
using Iot.Device.Sht4x;
using System.Text;

/*
This code takes a measurement of the temperature and humidity of the air and sends it in a Json format via LoRa, it also writes out received ones...
the Blink class can be found eg. in the upper directory... /R-IoT-Data_Hub/Dummy_LoRa_Node/Blink.cs
*/

namespace DOD
{
    public class SensorData
    {
        public double Temp { get; set; }
        public double Hum { get; set; }
    }
    public class Program
    {

        public static void Main()
        {

            Debug.WriteLine("Hello from nanoFramework!");
            //M5stack AtomS3 Lite has the button connected to pin 41, change dapending on your target platform
            GpioButton button = new GpioButton(buttonPin: 41);

            var tmp_hum = new SensorData();

            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(8, DeviceFunction.I2C2_DATA);
                Configuration.SetPinFunction(7, DeviceFunction.I2C2_CLOCK);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }

            I2cConnectionSettings I2C_settings = new(2, Sht4X.I2cDefaultAddress);
            using I2cDevice temp_hum_meter_device = I2cDevice.Create(I2C_settings);
            using Sht4X temp_hum_meter_snsr = new(temp_hum_meter_device);


            temp_hum_meter_device.WriteByte(0x96);//send soft reset to avoid initial CRC non-validity

            #region LoRa
            // Use COM2/COM3 to avoid conflict with the USB Boot port
            Configuration.SetPinFunction(2, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(1, DeviceFunction.COM2_RX);
            var lora = RYLR998.Create("COM2", 123);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    if (e.Data.StartsWith("{")){
                        SensorData rcvData = JsonConvert.DeserializeObject(e.Data, typeof(SensorData)) as SensorData;
                        Debug.WriteLine("Json we reaceived: " + rcvData.ToString()); 
                    } else {
                        Debug.WriteLine(e.Data.ToString());
                    }
                    Blink.Blinks(255, 255, 255, 1000, 1, 1);
                    
                    //Storage.new_formated_record(file_path, e.Data);
                };
            }
            #endregion

            // Write to debug if the button is pressed
            button.Press += (sender, e) =>
            {
                Debug.WriteLine($"Press");
                Sht4XSensorData data = null;

                try
                {
                    data = temp_hum_meter_snsr.ReadData(MeasurementMode.NoHeaterHighPrecision);
                    tmp_hum.Temp = data.Temperature.DegreesCelsius;
                    tmp_hum.Hum = data.RelativeHumidity.Percent;
                    /*Debug.WriteLine($"Temperature: {tmp_hum.Temp}\u00B0C");
                    Debug.WriteLine($"Relative humidity: {tmp_hum.Hum}%RH");*/
                }
                catch (Exception ex) { Debug.WriteLine("exeption: " + ex); }

                lora.SendAsync(0, JsonConvert.SerializeObject(tmp_hum));
                Blink.Blinks(0, 0, 255, 1000, 0.5, 2);
            };


            //Debug.WriteLine(Storage.read(file_path));
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
