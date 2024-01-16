
using Iot.Device.Button;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;



namespace NFAppAtomLite_Testing
{
    public class Program
    {
        static GpioPin releA = null;
        static int buttonOnPin = 32; // Adjust the pin number for the first button
        static int buttonOffPin = 26; // Adjust the pin number for the second button

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            GpioController controller = NewMethod();
            controller.OpenPin(buttonOnPin, PinMode.InputPullUp);
            controller.OpenPin(buttonOffPin, PinMode.InputPullUp);

            // Event handlers for button presses
            controller.RegisterCallbackForPinValueChangedEvent(buttonOnPin, PinEventTypes.Falling, ButtonOnPressed);
            controller.RegisterCallbackForPinValueChangedEvent(buttonOffPin, PinEventTypes.Falling, ButtonOffPressed);

            GpioController gpio = new GpioController();
            releA = gpio.OpenPin(21); //pozor pin 21 !!!!!


            Thread.Sleep(Timeout.Infinite);
        }
        static GpioController NewMethod()
        {
            return new GpioController();
        }
        private static void ButtonOnPressed(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Button to turn on relay pressed.");
            releA.SetPinMode(PinMode.Output);
        }

        private static void ButtonOffPressed(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Button to turn off relay pressed.");
            releA.SetPinMode(PinMode.Input);
        }
    }
}