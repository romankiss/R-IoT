using Iot.Device.Button;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

// $COM = [System.IO.Ports.SerialPort]::getportnames()
// nanoff --target ESP32_PICO --serialport COM29 --update --verbosity detailed  (overnight build)
// nanoff --target ESP32_WROOM_32 --serialport COM29 --update --verbosity detailed (very old version)
/*
System Information
HAL build info: nanoCLR running @ ESP32 built with ESP-IDF 6407ecb
  Target:   ESP32_PICO
  Platform: ESP32

Firmware build Info:
  Date:        Nov 17 2022
  Type:        MinSizeRel build, support for PSRAM
  CLR Version: 1.8.0.755
  Compiler:    GNU ARM GCC v8.4.0

 */
// https://docs.nanoframework.net/index.html
//
// Created by Roman Kiss, November 2022
//
namespace NFApp2
{
    public class Program
    {

        private static GpioController ioCtrl = new GpioController();
        public static void Main()
        {
            // PUK hardware
            var pinLedB = 26;
            var pinLedR = 27;
            var pinLedG = 14;
            var pinUserButton = 13;

            Debug.WriteLine("Hello from nanoFramework on the PUK!");

            // LEDs
            GpioPin ledR = ioCtrl.OpenPin(pinLedR, PinMode.Output);
            GpioPin ledG = ioCtrl.OpenPin(pinLedG, PinMode.Output);
            GpioPin ledB = ioCtrl.OpenPin(pinLedB, PinMode.Output);

            // Timers
            var timerLedR = new Timer((s) => ledR.Toggle(), null, 100, 1000);
            var timerLedG = new Timer((s) => ledG.Toggle(), null, 400, 1000);
            var timerLedB = new Timer((s) => ledB.Toggle(), null, 800, 1000);

            // Button
            GpioButton button = new GpioButton(buttonPin: pinUserButton);
            button.IsHoldingEnabled = true;
            button.Press += (sender, e) =>
            {
                Debug.WriteLine($"Press");
            };

            button.Holding += (sender, e) =>
            {
                switch (e.HoldingState)
                {
                    case ButtonHoldingState.Started:
                        timerLedR.Change(100, 250);
                        Debug.WriteLine($"Holding Started");
                        break;
                    case ButtonHoldingState.Completed:
                        Debug.WriteLine($"Holding Completed");
                        timerLedR.Change(100, 1000);
                        break;
                }
            };

            Thread.Sleep(Timeout.Infinite);
        }
    }
}


