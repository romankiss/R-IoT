using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Ssd13xx;
using Iot.Device.Button;

namespace LoRa
{

    public class Display
    {
        static Ssd1306 _display;
        static string some;

        public Display(Ssd1306 display)
        {
            _display = display;
        }
        public static void WriteMsg(string Message, int SplitLength = 16)
        {
            int SplitCount = 0;

            _display.ClearDirectAligned(0, 8, 128, 56);
            if (Message.Length > SplitLength)
            {
                SplitCount = Message.Length / SplitLength;
            }
            for (int i = 0; i < SplitCount; i++)
            {

                some = Message.Substring((SplitLength * i), SplitLength);
                _display.DrawString(0, ((i + 1) * 10), some);
                Debug.WriteLine(some);
            }
            some = Message.Substring(SplitLength * SplitCount);
            _display.DrawString(0, ((SplitCount + 1) * 10), some);
            Debug.WriteLine(some);
            _display.Display();
        }
    }
}
