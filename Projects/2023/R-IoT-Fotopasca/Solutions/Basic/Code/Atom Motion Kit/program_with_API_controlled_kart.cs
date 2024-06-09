

using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace NFAppAtomLite_Testing
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            Debug.WriteLine("Hello");
            TwoMotorKart kart = new TwoMotorKart();
            kart.Init();
             kart.goForward(180);
             Thread.Sleep(5000);
             kart.Stop();

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
                            if (kart != null && e.ButtonStatus)
                            {
                                
                                kart.goForward((byte)e.NewValue);
                                
                            }
                        }
                    }
                };
            }
            #endregion


            Thread.Sleep(Timeout.Infinite);
        }
        
    }

    public class TwoMotorKart
    {
        public static M5AtomicMotion motionbase;
        static byte _LeftMotorAKAServoChannel = 2;
        static byte _RightMotorAKAServoChannel = 0;

        public void Init(int DataPin = 38, int ClockPin = 39, byte RightMotorAKAServoChannel = 0, byte LeftMotorAKAServoChannel = 2)
        {

            _LeftMotorAKAServoChannel = LeftMotorAKAServoChannel;
            _RightMotorAKAServoChannel = RightMotorAKAServoChannel;
            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(DataPin, DeviceFunction.I2C2_DATA);//38     onboard:2
                Configuration.SetPinFunction(ClockPin, DeviceFunction.I2C2_CLOCK);//39    onbrd:1
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }
            //the base is hardwired to be on the second i2c bus
            I2cDevice i2c = new I2cDevice(new I2cConnectionSettings(2, M5AtomicMotion.DefaultI2cAddress));
            motionbase = M5AtomicMotion.Create(i2c);


        }

        public void goForward(int speed = 180)
        {
            motionbase.SetServoAngle(_RightMotorAKAServoChannel, (byte)speed);
            motionbase.SetServoAngle(_LeftMotorAKAServoChannel, (byte)(-speed+180));
        }

        

        public void Stop()
        {
            goForward(90);//!!!!!!
        }

    }
}
