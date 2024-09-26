// written by Roman Kiss, May 1st, 2024,
// version: 1.0.0.0 May 01,2024
//
// based on: 
// 
//
using System;
using System.Device.I2c;

namespace NFAppAtomLite_Testing
{
    public class I2cDeviceBase
    {
        private I2cDevice _i2cDevice = null;
        public I2cDevice I2cDevice => _i2cDevice;

        protected I2cDeviceBase(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        protected static I2cDevice IsDeviceConnected(I2cDevice i2cDevice)
        {
            return i2cDevice.WriteByte(0x07).Status == I2cTransferStatus.FullTransfer ? i2cDevice : null;
        }

        protected static I2cDevice IsDeviceConnected(int adress, int busId = 2, bool bScan = true)
        {
            I2cDevice i2c_device = new(new I2cConnectionSettings(busId, adress));
            var res = i2c_device.WriteByte(0x07);  
            {
                i2c_device = new(new I2cConnectionSettings(1, adress));
                res = i2c_device.WriteByte(0x07);
            }
            return res.Status == I2cTransferStatus.FullTransfer ? i2c_device : null;
        }

        protected void WriteBytes(byte reg, byte[] data)
        {
            byte[] outbuffer = new byte[data.Length + 1];
            outbuffer[0] = reg;
            data.CopyTo(outbuffer, 1);
            _i2cDevice.Write(new SpanByte(outbuffer));
        }
        protected void WriteByte(byte reg)
        {
            _i2cDevice.Write(new SpanByte(new byte[] { reg }));
        }
        protected void ReadBytes(byte reg, byte[] data)
        {
            _i2cDevice.WriteRead(new SpanByte(new byte[] { reg }), new SpanByte(data));
        }
        protected byte ReadByte(byte reg)
        {
            byte[] data = new byte[1];
            _i2cDevice.WriteRead(new SpanByte(new byte[] { reg }), new SpanByte(data));
            return data[0];
        }
    }
}
