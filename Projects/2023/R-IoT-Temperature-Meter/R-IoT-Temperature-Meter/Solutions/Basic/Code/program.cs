using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Sht3x;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;


namespace NFAppAtomLite_Testing
{
    public class Program
    {

        static Sk6812 neo = null;


        public static void Main()
        {

            GpioButton button = new GpioButton(buttonPin: 39);

            Debug.WriteLine("Hello from nanoFramework!");

            var neo = new Sk6812(26, 8);


            button.Press += (sender, e) =>
            {

                #region Temp&Hum  - sensor sht30   (RK: configuraciu pinov, sensorov, atd. treba urbot mimo "event handler", lebo ma byt vykonana len raz a nie pri kazdej udalosti)
                Configuration.SetPinFunction(32, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(26, DeviceFunction.I2C1_DATA);
                Sht3x sensorTH = new(new(new I2cConnectionSettings(1, 0x44)));
                Debug.WriteLine($"sensorTH: temperature[C]={sensorTH.Temperature.DegreesCelsius:F2}, humidity[%]={sensorTH.Humidity.Percent:F2}");
                #endregion


                int i = ((int)sensorTH.Temperature.DegreesCelsius);

                for (int n = -10; n <= i; n++)
                {
                    if (n < -5)
                    {
                        neo.Image.SetPixel(0, 0, 0, 0, 10);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= -5 && n < 0)
                    {
                        neo.Image.SetPixel(1, 0, 0, 0, 10);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 0 && n < 5)
                    {
                        neo.Image.SetPixel(2, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 5 && n < 10)
                    {
                        neo.Image.SetPixel(3, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 10 && n < 15)
                    {
                        neo.Image.SetPixel(4, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 15 && n < 20)
                    {
                        neo.Image.SetPixel(5, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 20 && n < 25)
                    {
                        neo.Image.SetPixel(6, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }

                    if (n >= 25)
                    {
                        neo.Image.SetPixel(7, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                    }
                }
            };

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
