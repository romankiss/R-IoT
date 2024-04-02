using MyLib;
using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.AtomLite;

namespace MatrixLed
{
    public class Program
    {

        public static SimpleHT16K33 ht16k33 = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            
            #region HT16K33
            ht16k33 = SimpleHT16K33.Create(AtomLite.GetGrove(0x70).ConnectionSettings.BusId, backpackWiring: BackpackWiring.Default);
            if (ht16k33 != null)
            {

                ht16k33.Init();
                ht16k33.SetBrightness(10);
                ht16k33.SetBlinkRate();
                ht16k33.Clear();
                ht16k33.ShowMatrixAsynch(0x5552492413080700, false, 400);
                
            }
            else
            {
                Debug.WriteLine("ajajajajaj");
            }

            Thread.Sleep(10000);
            ht16k33.ShowMatrixAsynch(0x0000000000000000, false, Circulate : false);
            ht16k33.Clear();
            



            #endregion
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}