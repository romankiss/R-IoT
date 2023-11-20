using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
//using Iot.Device.Hcsr04.Esp32;
using nanoFramework.AtomLite;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        //static Hcsr04 sonar = null;
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int ii = 0;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            //button = AtomLite.Button;
            button = new(39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.Press += Button_Press;

            //neo = AtomLite.NeoPixel;
            neo = new Sk6812(27, 3);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)
            neo.Image.SetPixel(1, 0, 0, 0, 10);
            neo.Update();

            // ultrasonic ranger sensor
            //sonar = new Hcsr04(32, 26);  //Grove trigger 32, echo 26;
            
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_Press(object sender, EventArgs e)
        {
            int i = ii++ & 0x3;
            Debug.WriteLine($"Button has been pressed, rgb = {i}");
            neo.Image.SetPixel(0, 0, (byte)(i==1?10:0), (byte)(i == 2 ? 10 : 0), (byte)(i == 3 ? 10 : 0));
            neo.Update();

             if (sonar != null) 
             {
                 if (sonar.TryGetDistance(out Length distance))
                     Debug.WriteLine($"Distance: {distance.Centimeters} cm");
                 else
                     Debug.WriteLine("Error reading sensor");
             }
        }
    }
}


//neo = new Sk6812(22, 1);
//neo.Image.SetPixel(0, 0, 0, (byte)(new Random()).Next(10), 0);
//neo.Update();
