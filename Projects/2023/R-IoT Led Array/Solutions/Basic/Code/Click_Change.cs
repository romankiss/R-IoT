using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.AtomLite;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static int ii = 0;
        static int jj = 0;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            button = new(39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.Press += Button_Press;

            neo = new Sk6812(32, 10);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)
            neo.Image.SetPixel(0, 0, 0, 0, 0);
            neo.Image.SetPixel(1, 0, 0, 0, 0);
            neo.Image.SetPixel(2, 0, 0, 0, 0);
            neo.Image.SetPixel(3, 0, 0, 0, 0);
            neo.Image.SetPixel(4, 0, 0, 0, 0);
            neo.Image.SetPixel(5, 0, 0, 0, 0);
            neo.Image.SetPixel(6, 0, 0, 0, 0);
            neo.Image.SetPixel(7, 0, 0, 0, 0);
            neo.Image.SetPixel(8, 0, 0, 0, 0);
            neo.Image.SetPixel(9, 0, 0, 0, 0);
            neo.Update();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_Press(object sender, EventArgs e)
        {
            if(jj == 10)
            {
                jj = 0;
                neo.Image.SetPixel(0, 0, 0, 0, 0);
                neo.Image.SetPixel(1, 0, 0, 0, 0);
                neo.Image.SetPixel(2, 0, 0, 0, 0);
                neo.Image.SetPixel(3, 0, 0, 0, 0);
                neo.Image.SetPixel(4, 0, 0, 0, 0);
                neo.Image.SetPixel(5, 0, 0, 0, 0);
                neo.Image.SetPixel(6, 0, 0, 0, 0);
                neo.Image.SetPixel(7, 0, 0, 0, 0);
                neo.Image.SetPixel(8, 0, 0, 0, 0);
                neo.Image.SetPixel(9, 0, 0, 0, 0);
                neo.Update();
            }
            else {
                if (ii % 4 == 0)
                {
                    ii = 1;
                    jj++;
                }
                else
                {
                    int i = ii++ & 0x3;
                    Debug.WriteLine($"Button has been pressed, rgb = {i}");
                    neo.Image.SetPixel(jj, 0, (byte)(i == 1 ? 10 : 0), (byte)(i == 2 ? 10 : 0), (byte)(i == 3 ? 10 : 0));
                    neo.Update();
                }
            }
        }
    }
}