using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;

namespace ESP_Car
{
    public class Program
    {
        private static M5AtomicMotion atombase;
        private static GpioButton button;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            Configuration.SetPinFunction(38, DeviceFunction.I2C2_DATA); //SDA
            Configuration.SetPinFunction(39, DeviceFunction.I2C2_CLOCK); //SCL
            //Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);
            //Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);
            
            //I2cDevice i2c_oled72x40 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C, I2cBusSpeed.StandardMode));

            I2cConnectionSettings setting = new I2cConnectionSettings(2, 0x38);
            I2cDevice i2c = I2cDevice.Create(setting);
            
            atombase = M5AtomicMotion.Create(i2c);
            Movement Car = new Movement(atombase);
            button = new GpioButton(41);

            Car.forward();
            Thread.Sleep(3000);
            Car.stop();
            Thread.Sleep(1000);
            Car.turnSquareLeft();
            Thread.Sleep(1000);
            Car.turnSquareRight();
            Thread.Sleep(1000);
            Car.turnLeft(45);
            Thread.Sleep(1000);
            Car.turnRight(135);
            Thread.Sleep(1000);
            Car.turnAround();

            Thread.Sleep(Timeout.Infinite);

        }
    }
}
