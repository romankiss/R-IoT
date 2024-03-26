using MyLib;
using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.AtomLite;
using Iot.Device.Button;

namespace MatrixLed
{
    public class Program
    {
        static GpioButton button = null;
        public static SimpleHT16K33 ht16k33 = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            GpioButton button = new GpioButton(buttonPin: 39);
            byte[] buffer = new byte[] { 0xfe, 0x02, 0xff };

            #region HT16K33
            ht16k33 = SimpleHT16K33.Create(AtomLite.GetGrove(0x70).ConnectionSettings.BusId, BackpackWiring.Backpack16x8);

            button.Press += (sender, e) =>
            {

                Debug.WriteLine($"Press");

                if (ht16k33 != null)
                {
                    ht16k33.Init();
                    ht16k33.SetBrightness(10);
                    ht16k33.SetBlinkRate();
                    ht16k33.Clear();

                    ht16k33.ShowMessage("SPSE Piestany",true, 100);
                    //ht16k33.ShowMatrix(0x183c7effffff6600, false, 100);
                    //ht16k33.ShowAndCirculateMessageAsync("", 250);
                    //ht16k33.ShowArray(buffer, bScrollLastCharacters: false);

                }
                else
                {
                    Debug.WriteLine("================");
                }
            };
           
            #endregion
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
