using System;
using System.Diagnostics;
using System.Threading;

using Iot.Device.Ws28xx.Esp32;


namespace Zko
{
    internal class Blink
    {
        static Sk6812 neo = new Sk6812(35, 1);

        public Blink()
        {
            neo = new Sk6812(35, 1);
           
        }


        #region Blink
        public static void Blinks(byte r, byte g, byte b, int periodIntMs = 100, double duty =0.5, int times = 1)
        {
          //starting to execute the blinking task in a new thread, period = time after the cycle starts repeating, duty = percentage/100 what amount of the period is the led on
            var Blink_thread = new Thread(() => { 
                for (int time = times; time > 0; --time)
                {
                    neo.Image.SetPixel(0, 0, r, g, b);
                    neo.Update();
                    Thread.Sleep((int)(periodIntMs*duty));//if period is eg. 2000ms = 2s and duty = 0.5 (50%), then the led will be on for 1s = period*duty = 2s*0.5 and off for 1s
                    neo.Image.Clear();
                    neo.Update();
                    if (time > 1) Thread.Sleep((int)(periodIntMs * (1-duty)));//if we should blink more than once, then add the delay when the led is off....eg.: if duty = 0.8 (80%), the led will be on for 80% of the period and off for 20% of the period(time off = period* (1-duty))
                }
            }){ Priority = ThreadPriority.BelowNormal };
            Blink_thread.Start();
        }
        #endregion




    }
}
