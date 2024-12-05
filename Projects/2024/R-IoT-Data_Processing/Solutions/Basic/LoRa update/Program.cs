using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Ssd13xx;
using Iot.Device.Button;
using nanoFramework.Json;
using LoRa;

namespace LoRa
{
    public class Program
    {
        static public int num = 0;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            #region Display
            Configuration.SetPinFunction(41, DeviceFunction.I2C2_CLOCK);
            Configuration.SetPinFunction(42, DeviceFunction.I2C2_DATA);

            I2cDevice i2c_oled128x64 = I2cDevice.Create(new I2cConnectionSettings(2, Ssd1306.DefaultI2cAddress));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled128x64);
            display.Font = new Sinclair8x8();
            display.ClearScreen();

            Display dis = new Display(display);
            #endregion

            #region button
            GpioButton button = new GpioButton(buttonPin: 10, debounceTime : TimeSpan.FromMilliseconds(330));
            GpioButton button1 = new GpioButton(buttonPin: 11, debounceTime: TimeSpan.FromMilliseconds(330));

            #endregion

            #region LoRa
            // Use COM2/COM3 to avoid conflict with the USB Boot port
            Configuration.SetPinFunction(43, DeviceFunction.COM2_TX);   //ESP32 S3 Atom 1
            Configuration.SetPinFunction(44, DeviceFunction.COM2_RX);   //ESP32 S3 Atom 2
            var lora = RYLR998.Create("COM2", 256, 7);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    try
                    {
                        var rcvData = JsonConvert.DeserializeObject(e.Data, typeof(CheckState2)) as CheckState2;
                    }
                    catch (Exception ex){}

                    Debug.WriteLine(e.Data.ToString());
                    Display.WriteMsg(e.Data);
                };

            }
            #endregion

            
            var Pins = new PinsValues();
            string js = Pins.PinVal();
            Debug.Write($"len={js.Length}, {js}");



            var task = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    lora.Send(0, "Ping");
                    Thread.Sleep(500);
                }
            })
            { Priority = ThreadPriority.BelowNormal };
            task.Start();
            task.Suspend();
            button1.ButtonDown += (sender, e) =>
            {
                if (num == 0)
                {

                    task.Resume();
                    num = 1;
                    display.DrawString(0, 0, "Start spaming");
                    display.Display();
                }
                else
                {

                    task.Suspend();
                    num = 0;
                    display.ClearDirectAligned(0, 0, 128, 8);
                    display.Display();
                }

            };

            button.ButtonDown += (sender, e) =>
            {
                Debug.Write($"{js}");
                lora.Send(0, js);
            };

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }

}
