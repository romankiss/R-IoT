using System;
using Windows.Devices.Gpio;
using Windows.System.Threading;

class Program
{
    static GpioPin pirPin;

    static void Main()
    {
        var gpio = GpioController.GetDefault();
        pirPin = gpio.OpenPin(12);

        // Nastavenie GPIO pinu ako vstupný
        pirPin.SetDriveMode(GpioPinDriveMode.Input);

        // Nastavenie časovača pre sledovanie zmien na GPIO pine
        ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(100));

        Console.ReadLine(); // Udržuje aplikáciu spustenú

    }

    private static void Timer_Tick(ThreadPoolTimer timer)
    {
        GpioPinValue pirValue = pirPin.Read();

        if (pirValue == GpioPinValue.High)
        {
            Console.WriteLine("Pohyb detekovaný!");
        }
    }
}
