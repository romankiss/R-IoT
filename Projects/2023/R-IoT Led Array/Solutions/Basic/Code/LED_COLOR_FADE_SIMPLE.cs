using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.AtomLite;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace LED_COLOR_FADE
{
    public class Program
    {
        static Sk6812 neo = null;
        static GpioButton button = null;
        static Random rnd = new Random();
        static int R = 0;
        static int G = 0;
        static int B = 0;

        static int max = 150;
        static int krok = 2;
        static int ms = 0 ;

        public static void Main()
        {

            Debug.WriteLine("Hello from nanoFramework!");

            button = new(39, debounceTime: TimeSpan.FromMilliseconds(200));
            button.Press += Button_Press;

            neo = new Sk6812(33, 1);    //AtomLite/Matrix 27 //AtomicPortABC 23/33 //Hat 22 //Grove 26(RGBLed), 32(RGBLedStick)

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Button_Press(object sender, EventArgs e)
        {
            neo.Image.SetPixel(0, 0, (byte)(R), (byte)(G), (byte)(B));
            neo.Update();

            for (int i = 0; i < 1; i++)
            {
                if (R == 0 && G == 0 && B == 0)
                {
                    while (R != max)
                    {
                        R += krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }
                if (R == max && G == 0 && B == 0)
                    {
                    while (G != max)
                    {
                        G += krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                    }
                if (R == max && G == max && B == 0)
                {
                    while (R != 0)
                    {
                        R -= krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }
                if (R == 0 && G == max && B == 0)
                {
                    while (B != max)
                    {
                        B += krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }
                if (R == 0 && G == max && B == max)
                {
                    while (G != 0)
                    {
                        G -= krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }
                if (R == 0 && G == 0 && B == max)
                {
                    while (R != max)
                    {
                        R += krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }
                if (R == max && G == 0 && B == max)
                {
                    while (B != 0)
                    {
                        B -= krok;
                        neo.Image.SetPixel(0, 0, Color.FromArgb(R, G, B));
                        neo.Update();
                        Thread.Sleep(ms);
                    }
                }

                neo.Image.Clear();
                neo.Update();
                break;
            }
        }
    }
}