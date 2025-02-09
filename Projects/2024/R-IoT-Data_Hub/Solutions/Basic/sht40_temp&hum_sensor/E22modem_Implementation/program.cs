using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Sht4x;
using System.Device.I2c;

namespace Ročníkový_projekt
{
    public class Program
    {
        
        public static Blink blink = new Blink();

        static E22 lora = null;
        static ushort broadcastAddress = 0xFFFF;
        const ushort loraAddress = 0x1234;  //55; //4660
        const byte loraNetworkId = 0x12;    // 850.125 + 18 = 868.125Mhz

        const ushort pub_period = 5000;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            blink.Blink_init();//dnt fgt to init
            

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
            Timer pubTimer = new Timer((s) => FireTelemetryData(), null, 0, pub_period);
            #region LoRa
            try
            {
                Configuration.SetPinFunction(2, DeviceFunction.COM3_TX);//19 //2  //ESP32 S3 Atom 1
                Configuration.SetPinFunction(1, DeviceFunction.COM3_RX);//22 //1  //ESP32 S3 Atom 2
                lora = E22.Create("COM3", loraAddress: loraAddress);
                if (lora != null)
                {
                    Debug.WriteLine("lora inited sucessfully");
                    lora.OnPacketReceived += (sender, e) =>
                    {
                        Debug.Write("RCVed Data: ");
                        for(int i =0; i < e.Data.Length; i++)
                        {
                            Debug.Write(((char)e.Data[i]).ToString());
                        }
                        Debug.WriteLine("");
                        Blink.Blinks(0, 255, 0, 500, 1, 1);
                    };
                }
                else
                {
                    Debug.WriteLine("lora is not inited");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCP: " + ex.ToString());
            }
            #endregion

            void FireTelemetryData()
            {
                try
                {   
                    if(lora != null && temp_hum_meter_snsr != null)
                    {
                        Sht4XSensorData data = temp_hum_meter_snsr.ReadData(MeasurementMode.NoHeaterHighPrecision);
                        // the place to put the code for handling an event from the pubTimer
                        lora.SendAsync(65535, $"T:{data.Temperature.DegreesCelsius}, H:{data.RelativeHumidity.Percent}");
                        Blink.Blinks(0, 0, 255, 500, 1, 1);
                    }
                    else
                    {
                        Debug.WriteLine("lora var is null");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EXCP: " + ex.ToString());
                }
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
