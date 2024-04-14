using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework;
using System.Device.Gpio;
using Iot.Device.Button;
using nanoFramework.AtomLite;
using Iot.Device.Ahtxx;
using Iot.Device.Bh1750fvi;
using MyLib;

namespace Matrix16x8
{
    public class Program
    {
        static GpioButton button = null;
        static Aht20 aht20 = null;
        static Bh1750fvi bh1750fvi = null;
        public static SimpleHT16K33 ht16k33 = null;

        public static void Main()
        {
            GpioController gpio = new GpioController();
            GpioButton button = new GpioButton(buttonPin: 39);

            #region PINS
            //hub 
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(25, DeviceFunction.I2C1_DATA);
            #endregion

            #region I2c
            //thermometer
            I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x38); // Replace with the correct I2C bus index and address
            I2cDevice i2cDevice = I2cDevice.Create(settings);
            aht20 = new Aht20(i2cDevice);

            //light sensore
            I2cConnectionSettings settings1 = new I2cConnectionSettings(1, 0x23);
            I2cDevice i2cdevice1 = I2cDevice.Create(settings1);
            bh1750fvi = new Bh1750fvi(i2cdevice1);

            ht16k33 = SimpleHT16K33.Create(AtomLite.GetGrove(0x70).ConnectionSettings.BusId, BackpackWiring.Backpack16x8);
            #endregion

            //====================================================//

            #region ht16k33
            ht16k33.Init();
            ht16k33.SetBrightness(10);
            ht16k33.SetBlinkRate();
            #endregion

            button.Press += (sender, e) =>
            {

                Debug.WriteLine($"Press");

                UpdateDisplay(ht16k33);

                

            };

            Thread.Sleep(Timeout.Infinite);
        }

        //========================================//

        private static void UpdateDisplay(SimpleHT16K33 ht16K33)
        {
            
            double temperature = RoundToDecimalPlaces(aht20.GetTemperature().DegreesCelsius, 2);
            double humidity = RoundToDecimalPlaces(aht20.GetHumidity().Percent, 2);
            double illuminance = bh1750fvi.Illuminance.Lux;


            DateTime currentTime = DateTime.UtcNow;
            DateTime time = currentTime.AddHours(1); //+1
            string timeString = time.ToString("HH:mm:ss");
            string dateString = time.ToString("dd/MM/yy");

            Debug.WriteLine("Time =  " + time); 
            Debug.WriteLine($"LUX = {illuminance}");
            Debug.WriteLine($"Temperature = {temperature}");
            Debug.WriteLine($"Humidity = {humidity}");


            //work in progress
            //sometimes it shows rounded values and sometimes it show not rounded values

            
            ht16k33.ShowMessage("Temp:" + temperature.ToString(), true,75);
            //ht16k33.ShowMessage("Hum:" + humidity.ToString(), true, 75);
            ht16K33.ShowMessage("Lux:" + Math.Round(illuminance).ToString(), true, 75);
            //ht16K33.ShowMessage("Date:" + dateString, true, 75);
            //ht16K33.ShowMessage("Time:" + dateString, true, 75);

            //ht16k33.ShowMatrixAsync(0x5552492413080700, false, 100);

            Debug.WriteLine("----------");
        }
        private static double RoundToDecimalPlaces(double value, int decimalPlaces)
        {
            double multiplier = Math.Pow(10, decimalPlaces);
            return Math.Round(value * multiplier) / multiplier;
        }


    }
}

