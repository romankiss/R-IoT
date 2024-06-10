// written by Florian Ferdinand, Matej Vyskocil, June 10, 2024,
// version: 1.0.0.0 June 10,2024

using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace ESP32_S3
{
    public class TwoMotorKart
    {
        public static M5AtomicMotion motionbase;
        static byte _leftServo = 3;
        static byte _rightServo = 0;

        public void Init(int DataPin = 38, int ClockPin = 39, byte rightServo = 0, byte leftServo = 2)
        {

            _leftServo = leftServo;
            _rightServo = rightServo;

            // Configure the I2C GPIOs used for the bus
            Configuration.SetPinFunction(DataPin, DeviceFunction.I2C2_DATA);//38     onboard:2
            Configuration.SetPinFunction(ClockPin, DeviceFunction.I2C2_CLOCK);//39    onbrd:1

            //the base is hardwired to be on the second i2c bus
            I2cDevice i2c = new I2cDevice(new I2cConnectionSettings(2, M5AtomicMotion.DefaultI2cAddress));
            motionbase = M5AtomicMotion.Create(i2c);


        }

        public void setSpeed(int speed = 180)
        {
            motionbase.SetServoAngle(_rightServo, (byte)speed);
            motionbase.SetServoAngle(_leftServo, (byte)(-speed + 180));
        }

        public void rotate(int angle = 180)
        {
            byte byteAngle = (byte)angle;
            motionbase.SetServoAngle(_rightServo, byteAngle);
            motionbase.SetServoAngle(_leftServo, (byte)byteAngle);

        }


    }
}
