 using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using CanSat.E22_900T22D.Wrapper;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using Iot.Device.Sht4x;//for temperature and humidity sensor
using Iot.Device.Vl53L0X;//for ToF sensor
using Iot.Device.Bmxx80;//for pressure sensor
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet; // for pressure and temperature units

namespace CanSat
{
    public class Program
    {


        //static int loopback_counter = 0;
        static E22 lora = null;
        static ushort broadcastAddress = 0xFFFF;
        const ushort loraAddress = 0x1234;  //55????;
        const byte loraNetworkId = 0x12;    // 850.125 + 18 = 868.125Mhz
        static Sht4X temp_hum_meter_snsr = null;
        static Vl53L0X sensorToF = null;
        static Bmp280 bmp280 = null; // BMP280 sensor


        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            //Parts of code provided by external author:
            // this code snippet shows usage of the event driven interests generated by Button and Timer
            // assembly:   nanoFramework.Iot.Device.Button
            //
            // more details for study:
            //   https://github.com/nanoframework/nanoframework.IoT.Device/blob/develop/devices/Button/README.md/
            //   https://docs.nanoframework.net/api/System.Threading.Timer.html


            // namespaces ---see upvise

            #region setup
            // ESP32 Hardware, board, chip, etc.
            const int pinButton = 41;
            const int pub_period = 10000;     // miliseconds
            const int pinCOM2_TX = 2;
            const int pinCOM2_RX = 1;
            const int pinI2C2_SDA = 8;
            const int pinI2C2_SCL = 7;
            const int pinI2C1_SDA = 6;
            const int pinI2C1_SCL = 5;

            Blink led = new Blink();//frustrating to discover that this little line is required to init the neo obj

            // Button setup
            GpioButton buttonM5 = new GpioButton(buttonPin: pinButton, debounceTime: TimeSpan.FromMilliseconds(333));

            

            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(pinI2C2_SDA, DeviceFunction.I2C2_DATA);
                Configuration.SetPinFunction(pinI2C2_SCL, DeviceFunction.I2C2_CLOCK);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }
            I2cConnectionSettings I2C_settings = new(2, Sht4X.I2cDefaultAddress);
            I2cDevice temp_hum_meter_device = I2cDevice.Create(I2C_settings);
            Sht4X temp_hum_meter_snsr = new(temp_hum_meter_device);


            temp_hum_meter_device.WriteByte(0x96);//send soft reset to avoid initial CRC non-validity

            try//for tof sensor
            {
                Debug.WriteLine("now configing");
                Configuration.SetPinFunction(pinI2C1_SDA, DeviceFunction.I2C1_DATA);//25 6
                Configuration.SetPinFunction(pinI2C1_SCL, DeviceFunction.I2C1_CLOCK);//21 5

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            I2cDevice i2c_tof = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
            var res = i2c_tof.WriteByte(0x07);
            sensorToF = new Vl53L0X(i2c_tof, 500);

            if (sensorToF != null)
            {
                sensorToF.HighResolution = false;
                sensorToF.Precision = Precision.HighPrecision;
                sensorToF.MeasurementMode = Iot.Device.Vl53L0X.MeasurementMode.Single;
            }
            else
            {
                Debug.WriteLine("ToF sensor initialization failed.");
            }


            // Initialize BMP280 sensor
            try
            {
                I2cDevice i2c_bmp280 = I2cDevice.Create(new I2cConnectionSettings(2, 0x76));//on the same bus as sht40 -------beware: 0x76 is the address, not the val stored in the library 
                bmp280 = new Bmp280(i2c_bmp280);
                bmp280.SetPowerMode(Bmx280PowerMode.Normal);
                bmp280.TemperatureSampling = Sampling.UltraHighResolution;
                bmp280.PressureSampling = Sampling.UltraHighResolution;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception initializing BMP280: " + ex.ToString());
            }


            // Timer setup
            Timer pubTimer = new Timer((s) => FireTelemetryData(), null, 0, pub_period);

            Configuration.SetPinFunction(pinCOM2_TX, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(pinCOM2_RX, DeviceFunction.COM2_RX);
            lora = E22.Create("COM2", loraAddress: loraAddress);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    Debug.WriteLine(e.Data.ToString());
                    Blink.Blinks(0, 255, 0, 500, 1, 1);                    //Debug.WriteLine($"nFmem_LoRaRcv={Memory.Run(true)}");
                    /*if (e.Data.ToString().StartsWith("6C6F6F706261636B"))    // loopback
                    {
                        int counter = Interlocked.Increment(ref loopback_counter);
                        lora.SendAsync(e.AddressID, $"Loopback_#{counter:D4}", loraNetworkId);
                    }*/
                    //WriteTextOnScreen($"E22: {e.AddressID:X4} {e.Data}", X: 0, Y: LCD_Y_StatusLine, fcolor: Color.GreenYellow, maxChars: LCD_NumberOfSmallChars, font: SmallFont);
                };
                //sensors.Append("LoRa ");-----prida�
            }
            #endregion





            // Button handler/callback
            buttonM5.Press += (sender, e) =>
            {
                // the place to put the code for handling an event from the buttonM5
                FireTelemetryData();
            };



            // Timer handler/callback
            void FireTelemetryData()
            {

                if (temp_hum_meter_snsr == null || sensorToF == null || bmp280 == null)
                {
                    Debug.WriteLine("Sensors are not initialized.");
                    return;
                }
                // the place to put the code for handling an event from the pubTimer
                Sht4XSensorData data = temp_hum_meter_snsr.ReadData(Iot.Device.Sht4x.MeasurementMode.NoHeaterHighPrecision);
                var dist = sensorToF.GetDistanceOnce();
                bmp280.TryReadPressure(out var bmpPressure);

                lora.Send(65535, "T" + Math.Floor( data.Temperature.DegreesCelsius ) + "H" + Math.Floor(data.RelativeHumidity.Percent) + "D" + dist + "P" + Math.Floor(bmpPressure.Hectopascals));
                Blink.Blinks(0, 0, 255, 100, 1, 1);
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
