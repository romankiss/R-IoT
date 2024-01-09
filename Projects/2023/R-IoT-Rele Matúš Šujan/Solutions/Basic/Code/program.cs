using Iot.Device.Button;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;



// https://docs.nanoframework.net/devicesdetails/Mfrc522/README.html
// https://github.com/nanoframework/nanoFramework.IoT.Device/blob/develop/devices/Mfrc522/samples/Program.cs

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        static GpioButton button = null;
        static GpioPin releA = null;
        static GpioPin releB = null;

        public static void Main()
        {
            int i;
            Debug.WriteLine("Hello from nanoFramework!");


            GpioController gpio = new GpioController();
            releA = gpio.OpenPin(32, PinMode.Output);         //Grove 32


            button = new(39, debounceTime: TimeSpan.FromMilliseconds(300));  //AtomLite 39 //Grove 26/32
            button.Press += (sender, e) =>
            {

                releA.Toggle();
            };
            Thread.Sleep(Timeout.Infinite);
        }
        private static void Button_Press(object sender, EventArgs e)
        {
            if (releA != null)
            {
                releA.Toggle();
            }
            if (releB != null)
            {
                releB.Toggle();
            }
        }
    }
}
