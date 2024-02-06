using System;
using System.Diagnostics;
using Iot.Device.Ssd13xx;
using Iot.Device.Adxl343Lib;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Numerics;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Iot.Device.Hcsr501;
using System.Device.Gpio;
using Iot.Device.Button;
using nanoFramework.AtomLite;
using Iot.Device.Ws28xx.Esp32;
using System.Threading;

namespace AccelDisplay
{
    public class Program
    {
        //static variables (added by RK)
        //static Ssd1306 display = null;
        //static Hcsr501 motionSensor = null;
        //static Sk6812 neo = null;
        //static int movementCounter = 0;

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

        public static Hcsr501 motionConfig() 
        {
            Hcsr501 motionSensor = new Hcsr501(23, PinNumberingScheme.Logical);
            return motionSensor;
        }

        public static void Main()
        {
            int movementCounter = 0;

            //Display
            Ssd1306 display = displayConfig();
            display.Orientation = DisplayOrientation.Landscape180;

            //Accel
            Adxl343 sensor = accelConfig();

            //Accel display
            Adxl343Display test1 = new Adxl343Display(256, 128, display, sensor);

            Adxl343Display test2 = new Adxl343Display(256, 128, display, sensor);

            //Motion sensor
            Hcsr501 motionSensor = motionConfig();
            //added by RK
            //motionSensor.Hcsr501ValueChanged += (s, e) =>
            //{
            //    if (e.PinValue == PinValue.High)
            //   {
            //        //Blink(Color.FromArgb(10, 10, 10));      // motion blink
            //        Interlocked.Increment(ref movementCounter);
            //    }
            // };

            //Button
            GpioButton button = new GpioButton(buttonPin: 39, debounceTime: TimeSpan.FromMilliseconds(200));
            // added by RK
            //button.Press += (sender, e) =>
            //{
            //    Debug.WriteLine("PRESSED!!");
            //    Interlocked.Exchange(ref movementCounter, 0);
            //};

            Vector3 v = new Vector3();
            Vector3 vOld = new Vector3(0, 0, 0);

            Sk6812 neo = new Sk6812(27, 3);
            neo.Image.SetPixel(1, 0, 10, 10, 10);
            neo.Update();

            display.ClearScreen();

            //Worked for 10 seconds, as all previous attempts, no longer works.
            //Heavy WIP
            // comment by RK: remove this task 
            new Thread(() =>
            {

            while (true) { 

                button.Press += (sender, e) =>
                {
                    Debug.WriteLine("PRESSED!!");
                    movementCounter = 0;
                };
                
                Thread.Sleep(Timeout.Infinite);
            }

            }).Start();


            Debug.WriteLine(movementCounter.ToString());

            while (true)
            {

                if (sensor.TryGetAcceleration(ref v))
                {


                    if (Math.Abs(v.X - vOld.X) > 17 || Math.Abs(v.Y - vOld.Y) > 17)
                     {
                           //Display text

                            display.ClearScreen();

                            test1.WriteVector(64, 64, v, "Display is functional.");
                            
                            test1.WriteVector(64, 48, v, "Movement counter:");    
                            //Succesfully counts movement
                            test1.WriteVector(64, 40, v, movementCounter.ToString());
                            display.Display();
                            vOld = v;

                    }

                }

                if (motionSensor.IsMotionDetected)
                {
                    //LED
                    Debug.WriteLine(movementCounter.ToString());
                    movementCounter++;
                }


            }




        }


    }

 } 

