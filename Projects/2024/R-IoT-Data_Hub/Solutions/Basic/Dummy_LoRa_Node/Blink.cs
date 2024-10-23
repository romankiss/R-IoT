using System;
using System.Diagnostics;
using System.Threading;

using Iot.Device.Ws28xx.Esp32;


namespace LoRa
{
    internal class Blink
    {
        static Sk6812 neo = new Sk6812(35, 1);

        public Blink()
        {
            neo = new Sk6812(35, 1);

        }


        #region Blink
        public static void Blinks(byte r, byte g, byte b, int periodIntMs = 100, double duty = 0.5, int times = 1)
        {
            var Blink_thread = new Thread(() => {
                for (int time = times; time > 0; --time)
                {
                    neo.Image.SetPixel(0, 0, r, g, b);
                    neo.Update();
                    Thread.Sleep((int)(periodIntMs * duty));
                    neo.Image.Clear();
                    neo.Update();
                    if (time > 1) Thread.Sleep((int)(periodIntMs * (1 - duty)));
                }
            })
            { Priority = ThreadPriority.BelowNormal };
            Blink_thread.Start();
        }
        #endregion




    }
}
