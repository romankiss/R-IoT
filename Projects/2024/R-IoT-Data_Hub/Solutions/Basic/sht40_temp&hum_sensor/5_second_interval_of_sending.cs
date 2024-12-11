using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Button;
using nanoFramework.Json;
using System.Device.I2c;
using Iot.Device.Sht4x;
using System.Text;
using Iot.Device.Ws28xx.Esp32;

/*prerobiť na event driven
pre-processing
processing
post-processing

timer envoke handler
lora async --- potálto ok
---odtálto sa bude robiť
BATCH publishing ... uložiť do logov samples with timestamp(short) a poslať naraz (sampling každú0,5s, publishing každých napr 5s)

 optimalizácie prenosu: 
byte/bit precision coding
temp nemusí byť ani int32, ani byte8, ale napr polka bytu (4b) na temp (16 možných rôznych hodnót - je jasné a predpokladané, že budú v rozmedí od napr. 10 do 35 stupňov)
 */


namespace DOD
{
    public class SensorData
    {
        public int Temp { get; set; }
        public int Hum { get; set; }
    }

    //work in progress
    public class Packet
    {
        public int counter { get; set; } //takes 4B
        public DateTime sendTime { get; set; }//idk compress somehow (h:min:sec 24*60*60)
        public SensorData payload { get; set; }//temp takes B/2, hum 1B (6b for vals eg 30 - 94 and 2b for precision of 1/4 %)
    }


    public class Program
    {
        
        public static void Stream_handler()
        {
            Console.WriteLine("Entering Stream handler");
        }

        public static void Main()
        {

            Debug.WriteLine("Hello from nanoFramework!");
            //M5stack AtomS3 Lite has the button connected to pin 41, change dapending on your target platform
            GpioButton button = new GpioButton(buttonPin: 41);
            Blink led = new Blink();//construct of the blink obj
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
                    
                    Debug.WriteLine(e.Data);
                    try
                    {
                        SensorData rcvData = JsonConvert.DeserializeObject(e.Data, typeof(SensorData)) as SensorData;
                        Debug.WriteLine("Json we reaceived: " + rcvData.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                            
                    
                    led.StartBlinking(255, 255, 255, 1000, 1, 1);
                    
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
                    tmp_hum.Temp = (int)data.Temperature.DegreesCelsius;
                    tmp_hum.Hum = (int)data.RelativeHumidity.Percent;
                    /*Debug.WriteLine($"Temperature: {tmp_hum.Temp}\u00B0C");
                    Debug.WriteLine($"Relative humidity: {tmp_hum.Hum}%RH");*/
                }
                catch (Exception ex) { Debug.WriteLine("exeption: " + ex); }
                var json = JsonConvert.SerializeObject(tmp_hum);
                lora.SendAsync(0, json);
                Debug.WriteLine(json);
                led.StartBlinking(0, 0, 255, 1000, 0.5, 2);
            };

            static TimerCallback timer_handler(Sht4X temp_hum_meter_snsr, SensorData tmp_hum, Blink led, RYLR998 lora )
            {
                Debug.WriteLine("Hellloo");

                Sht4XSensorData data = null;

                try
                {
                    data = temp_hum_meter_snsr.ReadData(MeasurementMode.NoHeaterHighPrecision);
                    tmp_hum.Temp = (int)data.Temperature.DegreesCelsius;
                    tmp_hum.Hum = (int)data.RelativeHumidity.Percent;
                    /*Debug.WriteLine($"Temperature: {tmp_hum.Temp}\u00B0C");
                    Debug.WriteLine($"Relative humidity: {tmp_hum.Hum}%RH");*/
                }
                catch (Exception ex) { Debug.WriteLine("exeption: " + ex); }
                var json = JsonConvert.SerializeObject(tmp_hum);
                lora.SendAsync(0, json);
                Debug.WriteLine(json);
                led.StartBlinking(0, 0, 255, 1000, 0.5, 2);

                return null;
            }
            try {
                Timer timer_watchdog = new Timer((s) => timer_handler(temp_hum_meter_snsr, tmp_hum, led, lora), null, 5000, 5000);
            }catch(Exception ex)
            {
                Debug.WriteLine("EX thrwn: " + ex.Message);
            }





            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
