using nanoFramework.Hardware.Esp32;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace CanSat-28.4
{
    public class Buzz
    {
        public static M5AtomicMotion m5base = null;

        public static bool Buzz_init(int pinI2C2_SCK, int pinI2C2_SDA, bool initI2C = true)
        {
            if(initI2C)
            {
                try
                {
                    Configuration.SetPinFunction(pinI2C2_SCK, DeviceFunction.I2C2_CLOCK);
                    Configuration.SetPinFunction(pinI2C2_SDA, DeviceFunction.I2C2_DATA);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error initing I2C2 BUS: " + ex.Message);
                    return false;
                }
            }

            //the base is hardwired to be on the second i2c bus
            I2cDevice motionBaseDev = new I2cDevice(new I2cConnectionSettings(2, M5AtomicMotion.DefaultI2cAddress));
            m5base = M5AtomicMotion.Create(motionBaseDev);
            if (m5base == null)
            {
                Debug.WriteLine("Motion base not initialized.");
                return false;
            }
            else { 
                return true;
            }
        }
        public static bool MakeABuzz(int duration_ms = 1000, int duty = 50)
        {
            if (m5base == null)
            {
                Debug.WriteLine("Motion base not initialized.");
                return false;
            }
            m5base.SetMotorSpeed(1, (sbyte)duty);
            Thread.Sleep(duration_ms);
            m5base.SetMotorSpeed(1, 0);


            return true;
        }

        public static void MakeABuzzAsync(int duration_ms = 1000, int duty = 50)
        {
            var task = new Thread(() =>
            {

                MakeABuzz(duration_ms, duty);
            })
            { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }
        
    }
}
