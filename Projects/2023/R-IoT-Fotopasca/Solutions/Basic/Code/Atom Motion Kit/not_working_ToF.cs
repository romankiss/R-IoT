/* 
when the pins seem to be corectly connected we get this:
++++ Exception System.IO.IOException - 0x00000000 (1) ++++
    ++++ Message: ReadStrobe timeout error
    ++++ Iot.Device.Vl53L0X.Vl53L0X::ReadStrobe [IP: 0028] ++++
    ++++ Iot.Device.Vl53L0X.Vl53L0X::GetSpadInfo [IP: 0066] ++++
    ++++ Iot.Device.Vl53L0X.Vl53L0X::Init [IP: 0084] ++++
    ++++ Iot.Device.Vl53L0X.Vl53L0X::.ctor [IP: 0022] ++++
    ++++ NFAppAtomLite_Testing.Program::Main [IP: 0030] ++++
Exception thrown: 'System.IO.IOException' in Iot.Device.Vl53L0X.dll
An unhandled exception of type 'System.IO.IOException' occurred in Iot.Device.Vl53L0X.dll
Additional information: ReadStrobe timeout error


,but when I flip the data with the clock pin I do not get the exception anymore. The measurement always returns 0 - so I guess that is also not correct.
*/

using Iot.Device.Vl53L0X;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;


namespace NFAppAtomLite_Testing
{
    public class Program
    {
        public static Vl53L0X sensorToF;

        public static void Main(string[] args)
        {
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);     //AtomicMotionBase SD-G19, G33
            Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);    //AtomicMotionBase SC-G22, G23
            Debug.WriteLine("Hello");
            /*TwoMotorKart kart = new TwoMotorKart();
            kart.Init();
            kart.goForward(180);
            Thread.Sleep(5000);
            kart.Stop();*/








            #region ToF 
            I2cDevice i2c_tof = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
            
            sensorToF = new Vl53L0X(i2c_tof, 1000);
            

            try
            {
                if (sensorToF != null)
                {
                    sensorToF.HighResolution = true;
                    sensorToF.Precision = Precision.ShortRange;
                    sensorToF.MeasurementMode = MeasurementMode.Single;
                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("EEEEEEEEEEEEEEEEEEEEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            };


            #endregion


            /*I2cDevice i2c_oled72x40 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C, I2cBusSpeed.StandardMode));
            var res = i2c_oled72x40.WriteByte(0x07);
            var display = new Ssd1306(i2c_oled72x40);*/


            /*
            display = new Ssd1306(i2c_oled72x40) { Width = 99, Height = 64, Font = new Sinclair8x8(), Orientation = Iot.Device.Ssd13xx.DisplayOrientation.Landscape180 };
            display.ClearScreen();
            display.Write(0, 1, "TEST", 1);
            display.Display();
            *//*
            #region Encoder

            Encoder sensorEncoder = Encoder.Create(busId: 1, pollingTimeInMilliseconds: 50);
            if (sensorEncoder != null)
            {
                sensorEncoder.OnChangeValue += (sender, e) =>
                {
                    Encoder encoder = sender as Encoder;
                    if (e.OldValue != e.NewValue || e.ButtonStatus)
                    {
                        Debug.WriteLine($"sensorEncoder: Val={e.NewValue}, button={e.ButtonStatus}");
                        if (e.NewValue < M5AtomicMotion.ServoAngleBack)
                            encoder.SetLEDColor(0, (byte)(e.NewValue), 0, 0);
                        else if (e.NewValue > M5AtomicMotion.ServoAngleAhead)
                            encoder.SetLEDColor(0, 0, 0, (byte)(e.NewValue));
                        else
                        {
                            encoder.SetLEDColor(0, 0, (byte)(e.NewValue), 0);
                            if (kart != null && e.ButtonStatus)
                            {

                                kart.goForward((byte)e.NewValue);
                                *//*display.ClearScreen();
                                display.Write(0, 1, "val: "+e.NewValue, 1);
                                display.Display();*//*

                                if (sensorToF != null)
                                {
                                    sensorToF.HighResolution = true;
                                    sensorToF.Precision = Precision.ShortRange;
                                    sensorToF.MeasurementMode = MeasurementMode.Continuous;
                                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");

                                    *//*display.Write(0, 0, "dist: " + sensorToF.Distance, 1);
                                    display.Display();*//*

                                }

                            }
                        }
                    }
                };
            }
            #endregion*/

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
