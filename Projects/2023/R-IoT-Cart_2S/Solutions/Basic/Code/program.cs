using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Button;
using ESP32_S3;
using System.Device.I2c;
using System.Device.Gpio;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;
using Iot.Device.Vl53L0X;
using Iot.Device.Ssd13xx;


//to do list:

//class koleso
//class kolesoA: koleso
//class kolesoB: koleso
//class vozik2D: kolesoA, kolesoB

//rotate
//90° = 323,5°
//1°  = 3,594°


namespace ESP32_S3
{
    public class Program
    {
        static GpioButton button = null;

        public static void Main(string[] args)
        {
            Debug.WriteLine("Hello");

            TwoMotorKart cart = new TwoMotorKart();
            cart.Init();

            //Thread.Sleep(5000);
            cart.rotate(323);


            Thread.Sleep(5000);
            cart.setSpeed(90);

            #region Encoder
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);     //AtomicMotionBase SD-G19, G33
            Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);    //AtomicMotionBase SC-G22, G23
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
                            if (cart != null && e.ButtonStatus)
                            {

                                cart.setSpeed((byte)e.NewValue);

                            }
                        }
                    }
                };
            }
            #endregion

            Thread.Sleep(Timeout.Infinite);
            //===========================//
        }

    }
}
