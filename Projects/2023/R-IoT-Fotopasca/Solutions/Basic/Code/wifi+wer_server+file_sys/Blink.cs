using nanoFramework.AtomLite;
using System;
//using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace websajt
{
    internal class Blink
    {
        public static void set(byte r = 0, byte g = 10, byte b = 0, int period_ms = 1000, int count = 1)
        {


            for (int i = 1; i <= count; i++)
            {
                AtomLite.NeoPixel.Image.SetPixel(0, 0, r, g, b);
                AtomLite.NeoPixel.Update();
                Thread.Sleep(period_ms);
                AtomLite.NeoPixel.Image.Clear();
                AtomLite.NeoPixel.Update();
                if (count > 1 && i != count)
                {
                    Thread.Sleep(period_ms);
                }
            }

        }
    }
}
