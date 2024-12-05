// written by Roman Kiss, May 1st, 2024,
// version: 1.0.0.0 May 01,2024
//
// based on: https://github.com/m5stack/M5Atomic-Motion/blob/master/src/M5AtomicMotion.cpp
// 
//
using System;
using System.Device.I2c;

namespace NFAppAtomLite_Testing
{
    public class M5AtomicMotion : I2cDeviceBase
    {
        // hardware constans
        // Servo angles
        public const byte ServoAngleBack = 0;
        public const byte ServoAngleStop = 90;
        public const byte ServoAngleAhead = 180;
        // Servo pulses
        public const int ServoPulseHighBack = 500;
        public const int ServoPulseStop = 1500;
        public const int ServoPulseHighAhead = 2500;
        // Motor speed
        public const sbyte MotorSpeedBack = -127;
        public const sbyte MotorSpeedStop = 0;
        public const sbyte MotorSpeedAhead = 127;

        // DefaultAddress
        public const byte DefaultI2cAddress = 0x38;

        public M5AtomicMotion(I2cDevice i2cDevice) : base(i2cDevice)
        {
        }
        public static M5AtomicMotion Create(I2cDevice i2cDevice)
        {
            return I2cDeviceBase.IsDeviceConnected(i2cDevice) != null ? new M5AtomicMotion(i2cDevice) : null;
        }
        public static M5AtomicMotion Create(int busId = 2, bool bScan = true)
        {
            var i2cDevice = I2cDeviceBase.IsDeviceConnected(DefaultI2cAddress, busId, bScan);
            return i2cDevice != null ? new M5AtomicMotion(i2cDevice) : null;
        }
        
        #region Servo
        public void SetServoAngle(byte servoChannel, byte angle = ServoAngleStop)
        {
            if (servoChannel > 3)
                throw new Exception($"The ServoChannel '{servoChannel}' is out of the range [0,1,2,3].");
           WriteBytes(servoChannel, new[] { angle });
        }

        public void SetServoPulse(byte servoChannel, ushort width = ServoPulseStop)
        {
            byte reg = (byte)(2 * servoChannel + 16);
            if (reg % 2 == 1 || reg > 32)
                throw new Exception($"The ServoChannel '{servoChannel}' is out of the range [0,1,2,3].");
            byte[] data = new[] { (byte)(width >> 8), (byte)(width & 0xFF) };
            WriteBytes(reg, data);
        }

        public byte GetServoAngle(byte servoChannel)
        {
            if (servoChannel > 3)
                throw new Exception($"The ServoChannel '{servoChannel}' is out of the range [0,1,2,3].");
             return ReadByte(servoChannel);
        }

        public ushort GetServoPulse(byte servoChannel)
        {
            byte reg = (byte)(2 * servoChannel + 16);
            if (reg % 2 == 1 || reg > 32)
                throw new Exception($"The ServoChannel '{servoChannel}' is out of the range [0,1,2,3].");
            byte[] data = new byte[2];
            ReadBytes(reg, data);
            return (ushort)((data[0] << 8) + data[1]);
        }
        #endregion

        #region Motor
        public void SetMotorSpeed(byte motorChannel, sbyte speed = MotorSpeedStop)
        {
            if (motorChannel > 1)
                throw new Exception($"The MotorChannel '{motorChannel}' is out of the range [0,1].");
            byte reg = (byte)(motorChannel + 32);
            WriteBytes(reg, new[] {(byte)speed});
        }

        public byte GetMotorSpeed(byte motorChannel)
        {
            if (motorChannel > 1)
                throw new Exception($"The MotorChannel '{motorChannel}' is out of the range [0,1].");
            byte reg = (byte)(motorChannel + 32);
            return ReadByte(reg);
        }
        #endregion
    }
}
