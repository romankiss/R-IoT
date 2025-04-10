#define AtomS3Lite      // XIASEEED_S3, XIASEEED_C3, AtomS3Lite, ...

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using CanSat.E22_900T22D.Wrapper;
//using CanSat.Waveshare;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using Iot.Device.Sht4x;//for temperature and humidity sensor
using Iot.Device.Vl53L0X;//for ToF sensor
using Iot.Device.Bmxx80;//for pressure sensor
using Iot.Device.Bmxx80.PowerMode;
using UnitsNet; // for pressure and temperature units
using Memory = nanoFramework.Runtime.Native.GC;
using Cansat;

namespace CanSat
{
    public class Program
    {

#if XIASEEED_S3
        //https://wiki.seeedstudio.com/xiao_esp32s3_getting_started/
        const int deviceId = 1;
        const ushort loraAddress = 0x905;
        const byte loraNetworkId = 0x12;  // 850.125 + 18 = 868.125Mhz
        const int pinButton = 0;
        const int pinNeo = -1;
        const int pinNeoPower = -1;
        const int pinLedA = 21;
        const int pinI2C1_SDA = 5;
        const int pinI2C1_SCK = 6;
        const int pinCOM2_TX = 43;       
        const int pinCOM2_RX = 44;
        const int pinCOM1_TX = 1;
        const int pinCOM1_RX = 2;
#elif AtomS3Lite
        // 
        const int deviceId = 1;//redefining to an int instead of a string to save bytes in transmission
        const ushort loraAddress = 0x1234;
        const byte loraNetworkId = 0x12;  // 850.125 + 18 = 868.125Mhz
        const int pinButton = 41;
        const int pinNeo = 35;
        const int pinNeoPower = -1;
        const int pinLedA = -1;
        const int pinI2C1_SDA = 2;      // Grove  the first i2C bus is being used to comm. with the Temp., Hum. and Press sensors as well as the dist. sensor
        const int pinI2C1_SCK = 1;      // Grove
        const int pinI2C2_SDA = 38;      // second i2C bus is being used to comm. with the base, that has the buzzer connected on the motor pins 
        const int pinI2C2_SCK = 39;        //also note, that the base MUST be on the second bus due to hardwiring reasons
        const int pinCOM1_TX = 6;       // HAT-G6, PORTC-G6,   G33, Grove-G2,  COM3 will hopefully be used for the GPS module
        const int pinCOM1_RX = 5;       // HAT-G8, PORTC-G5,   G19, Grove-G1,  
        const int pinCOM2_TX = 8;       // PORTB-G8, 
        const int pinCOM2_RX = 7;       // PORTB-G7
        //M5atomBase grove conn. can not be used for i2C, ony for UART
#endif

        //static int loopback_counter = 0;
        static int pub_counter = 0;
        const int pub_period = 1000;     // miliseconds
        const bool saveDataToLocalStorage = false;
        static E22 lora = null;
        static ushort BroadcastAddress = 0xFFFF;
        static Sht4X sensorTH = null;
        // static Vl53L0X sensorToF = null;
        static ToFSense sensorToF = null;
        static Bmp280 sensorBMP280 = null;
        static GpioController ioctrl = new GpioController();
        static string file_path = string.Empty;
        static GPS sensorGPS = null;
        static bool useGPS = true;
        static Blink led = null; // LED object for blinking



        public static class SensorData
        {
            public static DateTime Time { get; set; }
            public static int Counter { get; set; }
            public static double Temperature { get; set; }
            public static double Humidity { get; set; }
            public static int Distance { get; set; }
            public static double Pressure { get; set; }

            // GPS Data as a nested struct
            public static GpsData GPS { get; set; }

            public struct GpsData
            {
                public double Latitude { get; set; }
                public double Longitude { get; set; }
                public double Altitude { get; set; }
            }
        }



        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework! TEST");



            #region setup


            Blink led = new Blink();//frustrating to discover that this little line is required to init the neo obj
            if (led == null)
            {
                Debug.WriteLine("LED initialization failed.");
            }
            else
            {
                led.BlinkLedAsync(255, 255, 255, 1000, 1, 1);  // it must be async call
            }
                
                                                      // Button setup
            GpioButton buttonM5 = new GpioButton(buttonPin: pinButton, debounceTime: TimeSpan.FromMilliseconds(333));
            if (buttonM5 == null)
            {
                Debug.WriteLine("Button initialization failed.");
            }

            if (saveDataToLocalStorage)
            {
                file_path = "I:\\Measurement_" + DateTime.UtcNow.ToString("dd-MM-yyyy_HH-mm") + ".csv";
                Storage.init(file_path);
                Storage.list_files("I:\\", true);
                //Storage.PrintStorageInfo("I:\\");
                Storage.append(file_path, "Time, Temperature, Humidity, Distance, Pressure\r\n");
                Storage.read(file_path);
            }
            /* Buzz bz = new Buzz();
             Buzz.Buzz_init(pinI2C2_SCK, pinI2C2_SDA, true);*/
            #endregion


            #region SENSORS 

            

            try
            {
                //ioctrl.OpenPin(pinI2C1_SDA, PinMode.InputPullUp);  
                //ioctrl.OpenPin(pinI2C1_SCK, PinMode.InputPullUp); 
                Configuration.SetPinFunction(pinI2C1_SDA, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(pinI2C1_SCK, DeviceFunction.I2C1_CLOCK);
            }
            catch
            (Exception ex)
            {
                Debug.WriteLine("Error initing I2C BUS: " + ex.Message);
            }



            #region T&H 
            try
            {
                I2cDevice i2c_th = new(new I2cConnectionSettings(1, Sht4X.I2cDefaultAddress));     // Grove connector
                var resTH = i2c_th.WriteByte(0x07);
                if (resTH.Status == I2cTransferStatus.FullTransfer)
                {
                    sensorTH = new Sht4X(i2c_th);
                    i2c_th.WriteByte(0x96);         //send soft reset to avoid initial CRC non-validity
                    var data = sensorTH?.ReadData(Iot.Device.Sht4x.MeasurementMode.NoHeaterMediumPrecision);
                    if (data != null)
                    {
                        Debug.WriteLine($"sensorTH: temperature[C]={data.Temperature.DegreesCelsius:F2}, humidity[%]={data.RelativeHumidity.Percent:F2}");
                    }
                    else
                    {
                        Debug.WriteLine("TH sensor initialization failed.");
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine("Error initing T&H sensor: " + ex.Message);
            }

            #endregion

            #region ToF
            /* VL53L0X sensor, not used more,,, replaced by the waveshare sensor
            *I2cDevice i2c_tof = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
            var resToF = i2c_tof.WriteByte(0x07);
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
            }*/

            // Waveshare ToF Sensor
            I2cDevice i2c_tof = new(new I2cConnectionSettings(1, ToFSense.DefaultI2cAddress)); // Grove connector
            var res = i2c_tof.WriteByte(0x07);//check if it is the same as tof, coz here was a mistake previously, instead of i2c_bmp280 was tof used...
            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                sensorToF = new ToFSense(i2c_tof);
                Debug.WriteLine($"sensorTOF: {sensorToF.Distance} mm");
            }


            #endregion


            #region BMP280   
            I2cDevice i2c_bmp280 = new(new I2cConnectionSettings(1, 0x76)); // Grove connector
            var resBMP280 = i2c_bmp280.WriteByte(0x07); //.WriteByte(0x07);
            if (resBMP280.Status == I2cTransferStatus.FullTransfer)
            {
                sensorBMP280 = new Bmp280(i2c_bmp280);
                if (sensorBMP280 != null)
                {
                    sensorBMP280.SetPowerMode(Bmx280PowerMode.Normal);
                    sensorBMP280.TemperatureSampling = Sampling.UltraHighResolution;
                    sensorBMP280.PressureSampling = Sampling.UltraHighResolution;
                    sensorBMP280.TryReadPressure(out var bmpPressure);
                    Debug.WriteLine($"sensorBMP: {bmpPressure.Hectopascals:F2}hPa");
                }
                else
                {
                    Debug.WriteLine("BMP280 sensor initialization failed.");
                }
            }
            #endregion


            #region GPS

            // GPS is event driven in comparison to other sensors 
            if (useGPS)
            {
                Configuration.SetPinFunction(pinCOM1_RX, DeviceFunction.COM1_RX);
                Configuration.SetPinFunction(pinCOM1_TX, DeviceFunction.COM1_TX);
                sensorGPS = GPS.Create("COM1", 115200);
                /* if (sensorGPS != null)
                {
                    

                    sensorGPS.OnGpsReceived_GNGGA += (s, e) =>
                    {
                        bool isValid = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
                        //Debug.WriteLine($"GPS received GNGGA: {e.data}");
                        Debug.WriteLine($"GPS: {lat}, {lon}, {alt}");
                        SensorData.GPS = new SensorData.GpsData
                        {
                            Latitude = lat,
                            Longitude = lon,
                            Altitude = alt
                        };

                    };

                }*/
            }

            #endregion
            #endregion





            #region LoRa E22           
            Configuration.SetPinFunction(pinCOM2_TX, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(pinCOM2_RX, DeviceFunction.COM2_RX);
            lora = E22.Create("COM2", loraAddress: loraAddress);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    Debug.WriteLine(e.Data.ToString());
                    led.BlinkLedAsync(0, 255, 0, 100, 1, 1);  // it must be async call                 
                };
            }
            else
            {
                Debug.WriteLine("LoRa initialization failed.");
            }
            #endregion


            Timer pubTimer = new Timer((s) => FireTelemetryData(), null, 0, pub_period);

            // Button handler/callback
            buttonM5.Press += (sender, e) =>
            {
                // the place to put the code for handling an event from the buttonM5
                FireTelemetryData();
                led.BlinkLedAsync(0, 0, 255, 100, 1, 1);  // it must be async call
            };

            //Buzz.MakeABuzz(1000, 50);
           

            // Timer handler/callback
            void FireTelemetryData()
            {
                //Buzz.MakeABuzz(10, 50);
                //Debug.WriteLine("");
                Debug.WriteLine($"nFmem_FireStart={Memory.Run(true)}");

                try
                {
                    int counter = Interlocked.Increment(ref pub_counter);
                    string payload = $"#{counter}";
                    payload += $"I{deviceId}"; // add device ID to the payload
                    //SetLedByColor(0, 0, 2, 50);

                    if (sensorTH != null)
                    {
                        var data = sensorTH.ReadData(Iot.Device.Sht4x.MeasurementMode.NoHeaterMediumPrecision);
                        if (data != null)
                        {
                            var temperature = data.Temperature.DegreesCelsius;
                            var humidity = data.RelativeHumidity.Percent;
                            payload += $"T{temperature:F2}H{humidity:F2}";
                            SensorData.Temperature = temperature;
                            SensorData.Humidity = humidity;
                        }else payload += $"T-1H-1";
                    }else payload += $"T-1H-1";
                    if (sensorToF != null)
                    {
                        var d = sensorToF.Distance;
                        payload += $"D{d}";
                        SensorData.Distance = d;
                    }else payload += $"D-1";
                    if (sensorBMP280 != null)
                    {
                        sensorBMP280.TryReadPressure(out var bmpPressure);
                        payload += $"P{bmpPressure.Hectopascals:F2}";
                        SensorData.Pressure = bmpPressure.Hectopascals;
                    }else payload += $"P-1";

                    bool isValidGPS = false;
                    if (sensorGPS != null && sensorGPS.IsOpen)
                    {
                        // threadsafe parsing the GNGGA data received by serialport handler
                        isValidGPS = sensorGPS.TryParseGNGGA(out float lat, out float lon, out float alt);
                        if (isValidGPS)
                        {
                            payload += $"X{lat}";    //add latitude to the payload
                            payload += $"Y{lon}";    //add longitude to the payload
                            payload += $"Z{alt}";    //add altitude to the payload

                            // or
                            // payload += $"L{lat},{lon}";    //add latitude and longitude to the payload
                            // payload += $"A{alt}";          //add altitude to the payload
                            led.BlinkLedAsync(0, 255, 255, 100, 0.5, 1);  // it must be async call
                        }
                        else payload += $"X-1Y-1Z-1";

                    }
                    else payload += $"X-1Y-1Z-1";
                    // add more sensors to the payload    

                    // EOD (End of Data - Payload)
                    payload += $"\r\n";
                    //
                    Debug.WriteLine($">>> [{DateTime.UtcNow.ToString("hh:mm:ss.fff")}] {payload}");
                    //
                    if (lora != null && lora.IsOpen)
                    {
                        lora.Send(address: BroadcastAddress, data: payload);
                    }
                    else
                        Debug.WriteLine($"Device is not ready to publish message");

                    //SetLedByColor(Color.Black);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EXCEPTION: " + ex.InnerException?.Message ?? ex.Message);
                    //SetLedByColor(32, 0, 0);
                }
                finally
                {
                    Debug.WriteLine($"nFmem_FireEnd={Memory.Run(true)}");
                    Debug.WriteLine("");
                }

                
            }
            

            #region Ready for IoT (last step)
            Diag.PrintMemory("READY", true);
            //timer_watchdog?.Dispose();
            led.BlinkLedAsync(0, 20, 0, 250, 1,2);      // good light
            Thread.Sleep(Timeout.Infinite);
            #endregion

        }


        public static class Diag
        {
            public static void PrintMemory(string msg, bool compactHeap = false)
            {
                NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.All, out uint totalSize, out uint totalFreeSize, out uint largestBlock);
                Debug.WriteLine($"{msg}-> Total Mem {totalSize} Total Free {totalFreeSize} Largest Block {largestBlock}");
                Debug.WriteLine($"nF Mem {Memory.Run(compactHeap)} ");
            }
        }

    }
}
