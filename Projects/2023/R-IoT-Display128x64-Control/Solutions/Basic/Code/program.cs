using System;
using System.Diagnostics;
using Iot.Device.Ssd13xx;
using Iot.Device.Adxl343Lib;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Numerics;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;


namespace AccelDisplay
{
    public class Program
    {

        public static Ssd1306 displayConfig()
        {
            Configuration.SetPinFunction(25, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(21, DeviceFunction.I2C2_CLOCK);

            var i2cDisplay = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            Debug.WriteLine($"BUS SPEED DISPLAY: {i2cDisplay.ConnectionSettings.BusSpeed}");
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2cDisplay);
            display.Font = new Sinclair8x8();

            var overClock = new SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x0F);
            var adressingMode = new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Page);
            display.SendCommand(overClock);
            display.SendCommand(adressingMode);

            return display;
        }

        public static Adxl343 accelConfig()
        {
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C1_CLOCK);

            var i2cAccel = I2cDevice.Create(new I2cConnectionSettings(1, 0x53));
            Debug.WriteLine($"BUS SPEED ACCEL: {i2cAccel.ConnectionSettings.BusSpeed}");
            Adxl343 sensor = new Adxl343(i2cAccel, GravityRange.Range16);

            return sensor;
        }


        public static void Main()
        {
            //Display
            Ssd1306 display = displayConfig();
            display.Orientation = DisplayOrientation.Landscape180;

            //Accel
            Adxl343 sensor = accelConfig();

            //Accel display
            Adxl343Display test1 = new Adxl343Display(256, 128, display, sensor);

            Vector3 v = new Vector3();
            Vector3 vOld = new Vector3(0, 0, 0);

            display.ClearScreen();

            while (true)
            {

                if (sensor.TryGetAcceleration(ref v))
                    {

                     if (Math.Abs(v.X - vOld.X) > 17 || Math.Abs(v.Y - vOld.Y) > 17)
                     {
                           //Display text

                            display.ClearScreen();

                            test1.WriteVector(64, 64, v, "Displej funguje");
                            test1.WriteVector(64, 63, v, "vcelku neuveritelne");

                            display.Display();
                            vOld = v;
                     }
                }
            }




        }


    }

 } 
