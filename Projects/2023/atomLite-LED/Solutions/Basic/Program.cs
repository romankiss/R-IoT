using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.AtomLite;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFApp1
{
    public class Program
    {
        public static void Main()
        {

            Debug.WriteLine("Hello from nanoFramework!");

            Sk6812 neo = AtomLite.NeoPixel;
            neo.Image.SetPixel(0, 0, 40, 0, 0);
            neo.Update();

            Thread.Sleep(2000);

            neo.Image.SetPixel(0, 0, 0, 40, 0);
            neo.Update();

            Thread.Sleep(2000);

            neo.Image.SetPixel(0, 0, 0, 0, 40);
            neo.Update();

            Thread.Sleep(2000);

            neo.Image.SetPixel(0, 0, 0, 0, 0);
            neo.Update();

            //out pin = g26
            /*Button = AtomLite.Button;
            var neo = new 


            Thread.Sleep(Timeout.Infinite);

            */
        }
    }
}
