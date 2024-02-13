using Iot.Device.Button;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Sht3x;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;


namespace NFAppAtomLite_Testing
{
    public class Program
    {

        //static Sk6812 neo = null;


        public static void Main()
        {


            GpioButton button = new GpioButton(buttonPin: 39);

            Debug.WriteLine("Hello from nanoFramework!");

            var neo = new Sk6812(25, 8);


            button.Press += (sender, e) =>
            {
                neo.Image.Clear();

                #region Temp&Hum  - sensor sht30 
                Configuration.SetPinFunction(19, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(22, DeviceFunction.I2C1_DATA);
                Sht3x sensorTH = new(new(new I2cConnectionSettings(1, 0x44)));
                Debug.WriteLine($"sensorTH: temperature[C]={sensorTH.Temperature.DegreesCelsius:F2}, humidity[%]={sensorTH.Humidity.Percent:F2}");
                #endregion


                int i = ((int)sensorTH.Humidity.Percent);

                for (int n = 0; n <= i; n++)
                {
                    if (n < 12)
                    {
                        neo.Image.SetPixel(0, 0, 10, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(0, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(0, 0, 10, 0, 0);
                        neo.Update();
                    }

                    if (n >= 12 && n < 24)
                    {
                        neo.Image.SetPixel(1, 0, 9, 0, 1);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(1, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(1, 0, 9, 0, 1);
                        neo.Update();
                    }

                    if (n >= 24 && n < 36)
                    {
                        neo.Image.SetPixel(2, 0, 7, 0, 3);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(2, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(2, 0, 7, 0, 3);
                        neo.Update();
                    }

                    if (n >= 36 && n < 48)
                    {
                        neo.Image.SetPixel(3, 0, 6, 0, 4);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(3, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(3, 0, 6, 0, 4);
                        neo.Update();
                    }

                    if (n >= 48 && n < 60)
                    {
                        neo.Image.SetPixel(4, 0, 4, 0, 6);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(4, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(4, 0, 4, 0, 6);
                        neo.Update();
                    }

                    if (n >= 60 && n < 72)
                    {
                        neo.Image.SetPixel(5, 0, 2, 0, 8);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(5, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(5, 0, 2, 0, 8);
                        neo.Update();
                    }

                    if (n >= 72 && n < 84)
                    {
                        neo.Image.SetPixel(6, 0, 1, 0, 9);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(6, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(20);
                        neo.Image.SetPixel(6, 0, 1, 0, 9);
                        neo.Update();
                    }

                    if (n >= 84 && n <= 100)
                    {
                        neo.Image.SetPixel(7, 0, 0, 0, 10);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(7, 0, 0, 0, 0);
                        neo.Update();
                        Thread.Sleep(50);
                        neo.Image.SetPixel(7, 0, 0, 0, 10);
                        neo.Update();
                    }
                }
            };

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
