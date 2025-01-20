using nanoFramework.Json;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;

namespace LoRaE22PhaseChecker
{

    public class PinsValues
    {
        static int _rvo;
        static GpioController Phasepins;
        static int[] _lampPhasePins;
        static int[] _phasePins;
        static int _clockPin;
        static char[] lampPhaseValues = { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
        static char[] phaseValues = { '0', '0', '0' };
        static int phaseClock = '0';

        static byte humidity = 0;
        static short temperature = 0;


        public PinsValues(int[] lampPhasePins , int[] phasePins, int clockPin,int rvo)
        {
            _lampPhasePins = lampPhasePins;
            _phasePins = phasePins;
            _clockPin = clockPin;
            _rvo = rvo;


            Phasepins = new GpioController();

            foreach (int i in lampPhasePins)
            {
                Debug.WriteLine($"{i}");
                Phasepins.OpenPin(i, PinMode.InputPullUp);

            }

            foreach (int i in phasePins)
            {
                Debug.WriteLine($"{i}");
                Phasepins.OpenPin(i, PinMode.InputPullUp);
            }

            //Phasepins.OpenPin(clockPin, PinMode.InputPullUp);
        }

        public string PinVal()
        {
            for (int i = 0; i < _lampPhasePins.Length; i++)
            {
                lampPhaseValues[i] = Phasepins.Read(_lampPhasePins[i]) == PinValue.High ? '1' : '0';
                Debug.WriteLine($"{Phasepins.Read(_lampPhasePins[i])}");
            }

            for (int i = 0; i < _phasePins.Length; i++)
            {
                phaseValues[i] = Phasepins.Read(_lampPhasePins[i]) == PinValue.High ? '1' : '0';
                Debug.WriteLine($"{Phasepins.Read(_phasePins[i])}");
            }

            //phaseClock = Phasepins.Read(clockPin) == PinValue.High ? '1' : '0';

            var state = new Rvo()
            {
                S = new Sensors { H = humidity, T = temperature },
                SD = new SystemData { bat = 60, err = 0, R = _rvo },
                LP = lampPhaseValues.ConvertBitsToInt(),
                P = phaseValues.ConvertBitsToInt(),
                PC = phaseClock,
            };
            return state.GetData();
        }
    }
}
