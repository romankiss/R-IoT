using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Button;
using ESP32_S3;
using System.Device.Gpio;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;
using Distance_sensor;
using Iot.Device.Vl53L0X;
using System.Device.I2c;
using ESP32_S33;

namespace ESP32_S3
{
    public class Program
    {
        static M5AtomicMotion motionController;
        static GpioButton button = null;

        public static void Main(string[] args)
        {
            Debug.WriteLine("Hello");

            TwoMotorKart cart = new TwoMotorKart();
            cart.Init();

            //encoder handler
            EncoderHandler encoderHandler = new EncoderHandler(cart);
            encoderHandler.Initialize();

            I2cDevice i2cToF = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
            I2cDevice i2cMotor = I2cDevice.Create(new I2cConnectionSettings(2, 0x38));


            //ToFSensor and pass the I2C devices
            ToFSensor toFSensor = new ToFSensor(i2cToF, i2cMotor);


            //rotation
            Encoder encoder = Encoder.Create(busId: 1, pollingTimeInMilliseconds: 50);

            
            RotationHandler rotationHandler = new RotationHandler(motionController, encoder);



            while (true)
            {
                toFSensor.Update();
                Thread.Sleep(100);
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
