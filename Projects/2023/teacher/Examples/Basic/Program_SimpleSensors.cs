using Iot.Device.Button;
using Iot.Device.Sht3x;
using Iot.Device.Hcsr04;
using Iot.Device.Hcsr04.Esp32;
using Iot.Device.Hcsr501;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Ssd13xx;
using nanoFramework.AtomLite;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using UnitsNet;


// https://docs.nanoframework.net/devicesdetails/Mfrc522/README.html
// https://github.com/nanoframework/nanoFramework.IoT.Device/blob/develop/devices/Mfrc522/samples/Program.cs

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int ii = 0;
        static int motion_counter = 0;
        static Hcsr04 sonar = null;
        static GpioPin releA = null;
        static GpioPin releB = null;
        static Hcsr501 sensorPIR = null;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Timer pub_Timer = new Timer(TimerCallback, null, 10000, 60000);

            GpioController gpio = new GpioController();
            //releA = gpio.OpenPin(32, PinMode.Output);         //Grove 32
            //releB = gpio.OpenPin(26, PinMode.Output);         //Grove 26

            //button = AtomLite.Button;
            button = new(39, debounceTime: TimeSpan.FromMilliseconds(200));  //AtomLite 39 //Grove 26/32
            button.Press += Button_Press;

            //neo = AtomLite.NeoPixel;
            neo = new Sk6812(27, 3);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)
            neo.Image.SetPixel(1, 0, 0, 0, 10);
            neo.Update();

            #region ultrasonic ranger sensor
            //sonar = new Hcsr04(32, 26);  //Grove trigger 32, echo 26;
            #endregion

            #region PIR
            sensorPIR = new Hcsr501(26);    // G26 on the Hat, Atom Grove G32 
            sensorPIR.Hcsr501ValueChanged += (s, e) =>
            {
                if (e.PinValue == PinValue.High)
                {
                    // motion blink
                    neo.Image.SetPixel(0, 0, 10, 10, 10);
                    neo.Update() ;
                    Thread.Sleep(100);
                    neo.Image.SetPixel(0, 0, 0, 0, 0);
                    neo.Update();
                    //
                    Interlocked.Increment(ref motion_counter);
                }
            };
            #endregion  

            #region Temp&Hum   
            Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);   // Grove connector
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);    // grove connector
            Sht3x sensorTH = new(new(new I2cConnectionSettings(1, 0x44)));  // sensorAddress = 0x44
            Debug.WriteLine($"sensorTH: temperature[C]={sensorTH.Temperature.DegreesCelsius:F2}, humidity[%]={sensorTH.Humidity.Percent:F2}");
            #endregion


            #region Display 128x64, interface I2C connected via Grove connector 
            Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);   // Grove connector
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);    // Grove connector
            //
            I2cDevice i2c_oled128x64 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled128x64, DisplayResolution.OLED128x64);
            display.Orientation = DisplayOrientation.Landscape180;
            display.ClearScreen();
            display.Font =  new Sinclair8x8();
            display.Write(0, 0, "1234567890123456");
            display.Write(0, 1, "HELLO",2,true);
            display.Write(0, 3, "WORLD", 2, true);
            display.DrawHorizontalLine(0, 50, 127);
            display.Write(0, 7, "1234567890123456");
            byte[] buffer = { 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0xFF, 0xC3, 0xFF, 0xC3, 0xFF, 0xC3, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF };
            display.DrawBitmap(100, 20, 2, 15, buffer);
            display.DrawBitmap(100, 40, 1, 8, display.Font['X']);
            display.Display();
            #endregion

        




                
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_Press(object sender, EventArgs e)
        {
            //Led
            int i = ii++ & 0x3;
            Debug.WriteLine($"Button has been pressed, rgb = {i}");
            neo.Image.SetPixel(0, 0, (byte)(i==1?10:0), (byte)(i == 2 ? 10 : 0), (byte)(i == 3 ? 10 : 0));
            neo.Update();

            //ToF
            if (sonar != null)
            {
                if (sonar.TryGetDistance(out Length distance))
                    Debug.WriteLine($"Distance: {distance.Centimeters} cm");
                else
                    Debug.WriteLine("Error reading sensor");
            }

            // rele
            if(releA != null)
            {
                releA.Toggle();
            }

            // PIR
            //int motion = Interlocked.Exchange(ref motion_counter, 0);
            //Debug.WriteLine($"[{motion}]PIR change");
        }

        private static void TimerCallback(object state)
        {
            // PIR
            int motion = Interlocked.Exchange(ref motion_counter, 0);
            Debug.WriteLine($"[{motion}]PIR change");
        }
    }
}


//neo.Image.SetPixel(0, 0, 0, (byte)(new Random()).Next(10), 0);
//neo.Update();

// Timer pub_Timer = new Timer((s) => FireTelemetryData(device, component_name), null, 10000, pub_period);
// private static void FireTelemetryData(DeviceClient device, string component_name){}
