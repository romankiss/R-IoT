// written by Roman Kiss, May 1st, 2024,
// version: 1.0.0.0 May 01,2024
//          1.0.0.1 Dec 8, 2024
//          1.0.1.0 Apr 1, 2025  // adding WriteAndReadByte(s)
//
// based on: 
// 
//
using System;
using System.Device.I2c;

namespace NFAppLib
{
    public class I2cDeviceBase
    {
        private I2cDevice _i2cDevice = null;
        public I2cDevice I2cDevice => _i2cDevice;
        private object _i2cLock = new();

        protected I2cDeviceBase(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        protected static I2cDevice IsDeviceConnected(I2cDevice i2cDevice)
        {
            var result = i2cDevice.WriteByte(0x07);
            return result.Status == I2cTransferStatus.FullTransfer ? i2cDevice : null;
        }

        protected byte WriteAndReadByte(byte reg)
        {
            byte[] data = new byte[1];
            lock (_i2cLock)
            {
                _i2cDevice.Write(new SpanByte(new byte[] { reg }));
                _i2cDevice.Read(new SpanByte(data));
            }
            return data[0];
        }

        protected void WriteAndReadBytes(byte reg, byte[] data)
        {
            lock (_i2cLock)
            {
                _i2cDevice.Write(new SpanByte(new byte[] { reg }));
                _i2cDevice.Read(new SpanByte(data));
            }
        }

        protected void WriteBytes(ushort reg, byte[] data)
        {
            byte[] outbuffer = new byte[data.Length + 2];
            outbuffer[0] = (byte)(reg & 0x00ff);
            outbuffer[1] = (byte)((reg >> 8) & 0x00ff);
            data.CopyTo(outbuffer, 2);
            lock (_i2cLock)
                _i2cDevice.Write(new SpanByte(outbuffer));
        }
        protected void ReadBytes(ushort reg, byte[] data)
        {
            ReadBytes(new byte[] { (byte)(reg & 0x00ff), (byte)((reg >> 8) & 0x00ff) }, data);
        }
        protected void ReadBytes(ushort reg, byte[] data, ushort len)
        {
            lock (_i2cLock)
                _i2cDevice.WriteRead(new byte[] { (byte)(reg & 0x00ff), (byte)((reg >> 8) & 0x00ff) }, new SpanByte(data, 0, len));
        }
        protected byte ReadByte(ushort reg)
        {
            byte[] data = new byte[1];
            lock (_i2cLock)
                _i2cDevice.WriteRead(new byte[] { (byte)(reg & 0x00ff), (byte)((reg >> 8) & 0x00ff) }, new SpanByte(data));
            return data[0];
        }


        protected void WriteBytes(byte reg, byte[] data)
        {
            byte[] outbuffer = new byte[data.Length + 1];
            outbuffer[0] = reg;
            data.CopyTo(outbuffer, 1);
            lock (_i2cLock)
                _i2cDevice.Write(new SpanByte(outbuffer));
        }
        protected void WriteByte(byte reg)
        {
            lock (_i2cLock)
                _i2cDevice.Write(new SpanByte(new byte[] { reg }));
        }
        protected void ReadBytes(byte reg, byte[] data)
        {
            lock (_i2cLock)
                _i2cDevice.WriteRead(new SpanByte(new byte[] { reg }), new SpanByte(data));
        }
        protected void ReadBytes(byte[] reg, byte[] data)
        {
            lock (_i2cLock)
                _i2cDevice.WriteRead(new SpanByte(reg), new SpanByte(data));
        }
        protected byte ReadByte(byte reg)
        {
            byte[] data = new byte[1];
            lock (_i2cLock)
                _i2cDevice.WriteRead(new SpanByte(new byte[] { reg }), new SpanByte(data));
            return data[0];
        }
    }
}

