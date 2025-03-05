/*Trieda pre jednoduché blikanie trojfarebnou LEDkou vo veľajšom vlákne.*/
using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;//ovládanie LED

namespace CanSat//zmeniť podľa potreby
{
    internal class Blink
    {
        static Sk6812 neo = null;//vytvorenie objektu s názvom neo, ktorý je triedy Sk6812
        public Blink()
        {
            neo = new Sk6812(35, 1);//konštruktor triedy Blink, priradenie ovládaču 35. GPIO pin na ktorom je LED
        }

        #region Blink
        //public static Thread BlinkAsync(byte r, byte g, byte b, int periodIntMs = 100, double duty = 0.5, int times = 1)
        public static void Blinks(byte r, byte g, byte b, int periodIntMs = 100, double duty = 0.5, int times = 1)
        {
            /* Vysvetlenie parametrov:
             * r, g, b: farebný kód (0-255);
             * periodIntMs: perióda blikania v ms;
             * duty: podiel zapnutého stavu LED počas 1 periódy;
             * times: počet opakovaní bliknutia;
             */
            

            if (neo?.Image != null)
                {
                    for (int i = times; i > 0; --i)
                    {
                        neo.Image.SetPixel(0, 0, r, g, b);
                        neo.Update();
                        Thread.Sleep((int)(periodIntMs * duty));
                        neo.Image.Clear();
                        neo.Update();
                        if (i > 1) Thread.Sleep((int)(periodIntMs * (1 - duty)));
                    }
                }
            else
                {
                        // Handle the null case, e.g., log an error or throw an exception
                    throw new InvalidOperationException("The neo object or its Image property is not initialized.");
                }    

                
            
        }
        public static Thread BlinksAsync(byte r, byte g, byte b, int periodIntMs = 100, double duty = 0.5, int times = 1)
        {
            var Blink_thread = new Thread(() =>
            {//definíca nového vlákna
                Blink.Blinks(r, g, b, periodIntMs, duty, times);
            })
            { Priority = ThreadPriority.BelowNormal};
            Blink_thread.Start();//spustenie vlákna, v ktorom bežia súbežne s hlavným vláknom operácie vykonávajúce efekt blikania LED

            return Blink_thread; //added by RK (we need this reference to abort it)
        }

        #endregion
    }
}