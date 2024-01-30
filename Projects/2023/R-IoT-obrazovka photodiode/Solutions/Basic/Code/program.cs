using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using Iot.Device.Bh1750fvi;
using Iot.Device.Ssd13xx;
using Iot.Device.Button;
using System;
using Iot.Device.Ws28xx.Esp32;

namespace NFAppAtomLite_Testing
{
    public class Program
    {

        static Sk6812 neo = null;

        public static void Main()
        {
            Debug.WriteLine("not here , kein problem ");
            double illuminance;
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));

            //senzor
            Configuration.SetPinFunction(23, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(33, DeviceFunction.I2C1_CLOCK);
            I2cConnectionSettings settings = new I2cConnectionSettings(busId: 1, (int)I2cAddress.AddPinLow);
            I2cDevice device = I2cDevice.Create(settings);
            //display
            Configuration.SetPinFunction(22, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C2_CLOCK);
            //ledka
            Configuration.SetPinFunction(32, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(26, DeviceFunction.I2C1_CLOCK);

            I2cDevice i2c_oled128x64 = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled128x64);
            var sensor = new Bh1750fvi(device);


            var neo = new Sk6812(26, 8);


            //font do displaya
            display.Font = new Sinclair8x8();
            display.ClearScreen();

            button.Press += (sender, e) =>
            {

                illuminance = sensor.Illuminance.Value; //read illuminance(Lux)
                Debug.WriteLine($"LUX = {illuminance}");
                //zaokruhlenie na cele cislo 
                int y = (int)Math.Round(illuminance / 8);

                display.Write(0, 5, $"LUX = {y}");
                display.DrawHorizontalLine(0, 50, 127);

                display.Display();

                for (int n = -1; n <= y; n++)
                {
                    if (n == 0)
                    {
                        neo.Image.SetPixel(7, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }




                    if (n > 0 && n < 10)
                    {
                        neo.Image.SetPixel(0, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 10 && n < 20)
                    {
                        neo.Image.SetPixel(1, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 20 && n < 30)
                    {
                        neo.Image.SetPixel(2, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 30 && n < 40)
                    {
                        neo.Image.SetPixel(3, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 40 && n < 50)
                    {
                        neo.Image.SetPixel(4, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 50)
                    {
                        neo.Image.SetPixel(5, 0, 0, 10, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }
                }

            };



          Thread.Sleep(Timeout.Infinite);


        
            
        }
    }
}
