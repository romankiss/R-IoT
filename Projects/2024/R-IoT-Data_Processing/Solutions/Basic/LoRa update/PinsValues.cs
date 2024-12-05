using nanoFramework.Json;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;

namespace LoRa
{
    public class PinsValues
    {
        static GpioController Phasepins;
        static int[] pin = { 4, 5, 6, 8, 9, 12, 13, 14, 15, 21};
        static int[] phase = { 16, 17, 18};
        static char[] values = { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
        static char[] phasevalues = { '0', '0', '0' };

        public PinsValues() 
        {
            Phasepins = new GpioController();

            foreach (int i in pin)
            {
                Debug.WriteLine($"{i}");
                Phasepins.OpenPin(i, PinMode.InputPullUp);

            }


            foreach (int i in phase)
            {
                Debug.WriteLine($"{i}");
                Phasepins.OpenPin(i, PinMode.InputPullUp);

            }



        }
        public string PinVal()
        {
            for (int i = 0; i < pin.Length; i++)
            {
                if (Phasepins.Read(pin[i]) == PinValue.High)
                {
                    values[i] = '1';
                }
                else
                {
                    values[i] = '0';
                }
                Debug.WriteLine($"{Phasepins.Read(pin[i])}");
            }

            for (int i = 0; i < phase.Length; i++)
            {
                if (Phasepins.Read(phase[i]) == PinValue.High)
                {
                    phasevalues[i] = '1';
                }
                else
                {
                    phasevalues[i] = '0';
                }
                Debug.WriteLine($"{Phasepins.Read(phase[i])}");
            }

            var state = new CheckState2()
            {
                id = 12345,
                SysData = new SystemData { bat = 60, errmsg = 0 },
                LPh = values.ConvertBitsToInt(),
                Ph = phasevalues.ConvertBitsToInt(),
            };
            string js = state.SendData();
            return js;
        }
    }
}
