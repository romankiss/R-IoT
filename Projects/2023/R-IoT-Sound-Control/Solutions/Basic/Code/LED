using Iot.Device.Ws28xx.Esp32;
using nanoFramework.AtomLite;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFApp4
{
    public class Program
    {
        public static void Main()
        {
            Sk6812 neo = AtomLite.NeoPixel;
            while (true)
            {
                neo.Image.SetPixel(0, 0, 10, 40, 200);
                neo.Update();
                Thread.Sleep(2000);
                neo.Image.SetPixel(0, 0, 0, 40, 0);
                neo.Update();
                Thread.Sleep(2000);
                neo.Image.SetPixel(0, 0, 0, 40, 40);
                neo.Update();
                Thread.Sleep(2000);

            }
            //Thread.Sleep(Timeout.Infinite);
        }
    }
}
