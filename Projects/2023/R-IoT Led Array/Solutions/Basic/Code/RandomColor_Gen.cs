using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.AtomLite;
using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int ii = 0;

        public static void Main()
        {
            Random rnd = new Random();
            int R, G, B, i;
            Debug.WriteLine("Hello from nanoFramework!");

            button = AtomLite.Button;
            var neo = new Sk6812(32, 10);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)

            button.Press += (sender, e) =>
            {
                while (true) {

                    for (i = 0; i <= 10; i++)
                    {
                        R = rnd.Next(10);
                        B = rnd.Next(10);
                        G = rnd.Next(10);
                        neo.Image.Clear(); // if comented, previeous pixels stay loaded
                        neo.Image.SetPixel(i, 0, Color.FromArgb(R, G, B));

                        neo.Update();
                        Thread.Sleep(250);
                        if (i == 9)
                           break;
                    }
                    for(i = 8; i >= 1; i--)
                    {
                        R = rnd.Next(10);
                        B = rnd.Next(10);
                        G = rnd.Next(10);
                        neo.Image.Clear(); 
                        neo.Image.SetPixel(i, 0, Color.FromArgb(R, G, B));

                        neo.Update();
                        Thread.Sleep(250);
                        if (i == 1)
                           break;
                    }

                }
            };
            Thread.Sleep(Timeout.Infinite);
        }
    }
}