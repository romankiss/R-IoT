using nanoFramework.AtomLite;
using System;
//using System.Collections.Generic;
using System.Text;
using System.Threading;
using static System.Math;

namespace websajt
{
    internal class Blink
    {
        public static void set(byte r = 0, byte g = 10, byte b = 0, int frequency = 1, double duty = 0.5, int count = 1)
        {
            //frequency: how much blinking cycles to do in one second (one blinking cycle means time it is on (if duty is 0,5 it is on half the time of the cycle) plus time it is off (1-duty * 100 is the percent of time the leds are of)))
            //count: how much blinking cycles to make


            for (int i = 1; i <= count; i++)
            {
                AtomLite.NeoPixel.Image.SetPixel(0, 0, r, g, b);
                AtomLite.NeoPixel.Update();
                Thread.Sleep((int)Math.Round((1000 / frequency) * duty));
                AtomLite.NeoPixel.Image.Clear();
                AtomLite.NeoPixel.Update();
                if (count > 1 && i != count)
                {
                    Thread.Sleep((int)Math.Round((1000 / frequency) * (1 - duty)));
                }
            }

        }
    }
}
