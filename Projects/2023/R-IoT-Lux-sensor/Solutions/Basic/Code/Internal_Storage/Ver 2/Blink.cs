using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.AtomLite;
using Iot.Device.Ws28xx.Esp32;


namespace websajt
{
    internal class Blink
    {
        static Sk6812 neo = null;

        public Blink()
        {
            neo = new Sk6812(27, 3);
            neo = AtomLite.NeoPixel;
        }


        #region Blink
        public static void Blinks(byte r, byte g, byte b, int periodIntMs = 100, int times = 1)
        {
            for (int time = times; time > 0; --time)
            {
                neo.Image.SetPixel(0, 0, r, g, b);
                neo.Update();
                Thread.Sleep(periodIntMs);
                neo.Image.Clear();
                neo.Update();
                if (time > 1) Thread.Sleep(periodIntMs);
            }

        }
        #endregion




    }
}