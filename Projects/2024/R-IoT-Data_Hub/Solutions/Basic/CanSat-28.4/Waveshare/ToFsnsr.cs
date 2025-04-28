// Driver for ToF Sense from Waveshare.
// written by Roman Kiss, March 4th, 2025,
// version: 1.0.0.0 March 4,2025
//
// based on: 
// https://www.waveshare.com/wiki/TOF_Laser_Range_Sensor_(C)#Demo
//



using CanSat-28.4;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;

namespace CanSat-28.4
{
    public class ToFSense : I2cDeviceBase
    {
        public const byte DefaultI2cAddress = 0x08;         // 7-bit slave address = ID + 0x08 ID = 0
        public const byte DefaultI2cAddress1 = 0x09;        // 7-bit slave address = ID + 0x08 ID = 1

        public const byte TOF_ADDR_DIS_STATUS = 0x28;       // Distance status indication variable address
        public const byte TOF_ADDR_DIS = 0x24;              // Distance variable address
        public const byte IIC_CHANGE_TO_UART_DATA = 0x00;   // Change the communication mode to the byte data that needs to be sent by UART

        public ToFSense(I2cDevice i2cDevice) : base(i2cDevice)
        {
        }

        public void ChangeModeToUART()
        {
            base.WriteByte(IIC_CHANGE_TO_UART_DATA);
        }

        /// <summary>
        /// Get distance in milimeters
        /// </summary>
        /// <returns>return -1 for invalid distance status</returns>
        public int Distance
        {
            get
            {
                byte[] buffer = new byte[256];
                base.ReadBytes(0x00, buffer); // Read all sensor data 

                byte status = buffer[TOF_ADDR_DIS_STATUS];
                return status == 0x01 ? BitConverter.ToInt32(buffer, TOF_ADDR_DIS) : -1;
            }
        }
    }
}