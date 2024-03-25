using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;
//using System.Device.Adc;
//using Windows.Devices.Adc;


namespace Sound
{
    public class Program
    {
        static GpioController gpio_controller;
        static GpioPin digital_pin;
        //static GpioPin analog_pin;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            gpio_controller = new GpioController();
            digital_pin = gpio_controller.OpenPin(25, PinMode.Input);

            /*AdcController adc = AdcController.GetDefault();
            AdcChannel ac4 = adc.OpenChannel(0);*/
            //analog_pin = gpio_controller.OpenPin(21, PinMode.Input);

            while (true)
            {
                Debug.WriteLine($"Digital val.: {digital_pin.Read()}");
                //Debug.WriteLine($"Analog val.: {ac4.ReadValue()}");
                Thread.Sleep( 100 );
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
