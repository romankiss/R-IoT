using Iot.Device.Button;
//using Iot.Device.Sht3x;
using Iot.Device.Hcsr04;
//using Iot.Device.Hcsr04.Esp32;
using Iot.Device.Hcsr501;
using Iot.Device.Ws28xx.Esp32;
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
        
        public static GpioButton button = null;
        static int ii = 0;
        public static Hcsr04 sonar = null;
       

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            //Timer pub_Timer = new Timer(TimerCallback, null, 10000, 5000);

            GpioController gpio = new GpioController();
            //releA = gpio.OpenPin(32, PinMode.Output);         //Grove 32
            //releB = gpio.OpenPin(26, PinMode.Output);         //Grove 26
            #region ultrasonic ranger sensor
            sonar = new Hcsr04(33, 23);  //Grove trigger 32, echo 26;
            #endregion
            button = new(39, debounceTime: TimeSpan.FromMilliseconds(200));  //AtomLite 39 //Grove 26/32
            button.Press += (sender, e) => {
                if (sonar.TryGetDistance(out Length distance))
                {
                    
                    Debug.WriteLine($"Distance: {distance.Centimeters} cm");
                }
                else
                {
                    Debug.WriteLine("Error reading sensor");
                }
            };

            //neo = AtomLite.NeoPixel;
            

            

           
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_Press(object sender, EventArgs e)
        {
            if (sonar.TryGetDistance(out Length distance))
            {
            
                Thread.Sleep(1000);
                Debug.WriteLine($"Distance: {distance.Centimeters} cm");
            }
            else
            {
                Debug.WriteLine("Error reading sensor");
            }

            
            //Led
            int i = ii++ & 0x3;
            Debug.WriteLine($"Button has been pressed, rgb = {i}");
        }
    }
}


//neo.Image.SetPixel(0, 0, 0, (byte)(new Random()).Next(10), 0);
//neo.Update();

// Timer pub_Timer = new Timer((s) => FireTelemetryData(device, component_name), null, 10000, pub_period);
// private static void FireTelemetryData(DeviceClient device, string component_name){}
