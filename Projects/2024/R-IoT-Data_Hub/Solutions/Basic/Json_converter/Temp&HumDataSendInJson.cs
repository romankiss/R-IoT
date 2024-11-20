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


/*however, after thesting the deserializing(json->class) capabilities while receiving with some test data of: {"Temp":21,"Hum":50}, the following exception is thrown:
LoRaEvent: +RCV=2000,20,{"Temp":21,"Hum":50},-5,11

{"Temp":21
    ++++ Exception System.IndexOutOfRangeException - 0xa9000000 (3) ++++
    ++++ Message: 
    ++++ nanoFramework.Json.JsonConvert::GetNextTokenInternal [IP: 0203] ++++
    ++++ nanoFramework.Json.JsonConvert::GetNextToken [IP: 0005] ++++
    ++++ nanoFramework.Json.JsonConvert::ParseValue [IP: 0006] ++++
    ++++ nanoFramework.Json.JsonConvert::ParseObject [IP: 0039] ++++
    ++++ nanoFramework.Json.JsonConvert::Deserialize [IP: 001b] ++++
    ++++ nanoFramework.Json.JsonConvert::Deserialize [IP: 0011] ++++
    ++++ nanoFramework.Json.JsonConvert::DeserializeObject [IP: 0015] ++++
    ++++ nanoFramework.Json.JsonConvert::DeserializeObject [IP: 0008] ++++
    ++++ DOD.Program+<>c::<Main>b__0_0 [IP: 0027] ++++
    ++++ DOD.RYLR998+<>c__DisplayClass14_0::<Create>b__0 [IP: 0048] ++++
    ++++ System.IO.Ports.SerialPort::OnSerialDataReceivedInternal [IP: 0029] ++++
    ++++ System.IO.Ports.SerialDeviceEventListener::OnEvent [IP: 002e] ++++
    ++++ nanoFramework.Runtime.Events.EventSink::ProcessEvent [IP: 0023] ++++
    ++++ nanoFramework.Runtime.Events.EventSink::EventDispatchCallback [IP: 0014] ++++
Exception thrown: 'System.IndexOutOfRangeException' in nanoFramework.Json.dll
LoRaEvent: Exception was thrown: System.IndexOutOfRangeException

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
