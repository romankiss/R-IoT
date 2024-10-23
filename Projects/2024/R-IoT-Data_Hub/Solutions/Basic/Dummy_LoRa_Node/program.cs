using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Button;

namespace LoRa
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            //M5stack AtomS3 Lite has the button connected to pin 41, change dapending on your target platform
            GpioButton button = new GpioButton(buttonPin: 41);




            #region LoRa
            // Use COM2/COM3 to avoid conflict with the USB Boot port
            Configuration.SetPinFunction(2, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(1, DeviceFunction.COM2_RX);
            var lora = RYLR998.Create("COM2", 123);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    if (e.Data.StartsWith("loopback"))
                        lora.SendAsync(e.AddressID, e.Data);
                    Debug.WriteLine(e.Data + "  from: " + e.AddressID);
                    Blink.Blinks(0, 255, 100, 1000, 1, 1);
                };
            }
            #endregion

            // Write to debug if the button is pressed
            button.Press += (sender, e) =>
            {
                Debug.WriteLine($"Press");
                lora.SendAsync(0, "PING");
            };

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
