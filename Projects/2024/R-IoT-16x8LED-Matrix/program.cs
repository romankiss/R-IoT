using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using MyLib;
using System.Device.I2c;
using System.Device.Gpio;
using nanoFramework.Hardware.Esp32;

namespace _16x8_led_matrix
{
    public class Program
    {
        static GpioButton button = null;
        public static SimpleHT16K33 ht16k33 = null;
        public static void Main()
        {
            Thread.Sleep(5000);

            Debug.WriteLine("Hello from nanoFramework!");

            GpioButton button = new GpioButton(buttonPin: 41);
                        

            #region HT16K33
            Configuration.SetPinFunction(2, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(1, DeviceFunction.I2C2_CLOCK);

            I2cConnectionSettings i2csettings = new I2cConnectionSettings(2, SimpleHT16K33.DefaultI2cAddress);
            I2cDevice i2cdevice = I2cDevice.Create(i2csettings);

          
            ht16k33 = new SimpleHT16K33(i2cdevice);
            ht16k33 = SimpleHT16K33.Create(2, BackpackWiring.Backpack16x8);

            Debug.WriteLine("Windows");
            
            
            button.Press += (sender, e) =>
            {

                Debug.WriteLine($"Press");

                if (ht16k33 != null)
                {
                    ht16k33.Init();
                    ht16k33.SetBrightness(10);
                    ht16k33.SetBlinkRate();
                    ht16k33.Clear();

                    ht16k33.ShowMessage("SPSE Piestany", true, 100);
                    ht16k33.ShowMatrix(0x183c7effffff6600, false, 100);

                }
                else
                {
                    Debug.WriteLine("========ERROR========");
                }
            
            };
            
            #endregion

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
