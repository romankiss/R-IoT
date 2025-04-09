// Driver for EXT-IO2 device from M5Stack.
// written by Roman Kiss, March 28th, 2025,
// version: 1.0.0.0 March 28,2025
//
// based on: 
// https://github.com/m5stack/M5Unit-EXTIO2/tree/main
//


using System;
using System.Text;
using System.IO.Ports;
using System.Device.I2c;
using System.Diagnostics;

namespace NFAppLib
{
    public enum ExtioIoMode
    {
        DigitalInputMode = 0,
        DigitalOutputMode,
        AdcInputMode,
        ServoCtlMode,
        RgbLedMode,
        PWM
    }

    public enum ExtioAnalogReadMode
    {
        _8bit = 0,
        _12bit
    }

    public class M5_EXTIO2 : I2cDeviceBase
    {
        public const byte DefaultI2cAddress = 0x45;

        const byte EXTIO2_MODE_REG            = 0x00;
        const byte EXTIO2_OUTPUT_CTL_REG      = 0x10;
        const byte EXTIO2_OUTPUTS_CTL_REG     = 0x18;
        const byte EXTIO2_DIGITAL_INPUT_REG   = 0x20;
        const byte EXTIO2_DIGITAL_INPUTS_REG  = 0x28;
        const byte EXTIO2_ANALOG_INPUT_8B_REG = 0x30;
        const byte EXTIO2_ANALOG_INPUT_12B_REG= 0x40;
        const byte EXTIO2_SERVO_ANGLE_8B_REG  = 0x50;
        const byte EXTIO2_SERVO_PULSE_16B_REG = 0x60;
        const byte EXTIO2_RGB_24B_REG         = 0x70;
        const byte EXTIO2_PWM_DUTY_CYCLE_REG  = 0x90;
        const byte EXTIO2_PWM_FREQUENCY_REG   = 0xA0;
        const byte EXTIO2_FW_VERSION_REG      = 0xFE;
        const byte EXTIO2_ADDRESS_REG         = 0xFF;

        public M5_EXTIO2(I2cDevice i2cDevice, ExtioIoMode mode = ExtioIoMode.DigitalInputMode) : base(i2cDevice)
        {
            SetAllPinMode(mode);
            Debug.WriteLine($"M5_EXTIO2 Addr: 0x{GetDeviceAddr():X2}, version: 0x{GetVersion():X2}");
        }

        public void Close()
        {
            base.I2cDevice.Dispose();
        }

        public byte GetVersion()
        {
            return base.WriteAndReadByte(EXTIO2_FW_VERSION_REG);
        }

        public void SetAllPinMode(ExtioIoMode mode)
        {
            byte[] data = new byte[8];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)mode;
            }
            base.WriteBytes(EXTIO2_MODE_REG, data);
        }

        public void SetPinMode(byte pin, ExtioIoMode mode)
        {
            pin &= 0x07;    // valid only for 8 pins
            base.WriteBytes((byte)(EXTIO2_MODE_REG + pin), new byte[] { (byte)mode });
        }
        public byte GetPinMode(byte pin)
        {
            pin &= 0x07;    // valid only for 8 pins
            return base.WriteAndReadByte((byte)(EXTIO2_MODE_REG + pin));
        }

        public void SetDeviceAddr(byte addr)
        {
            base.WriteBytes(EXTIO2_ADDRESS_REG, new byte[] { addr });
        }

        public byte GetDeviceAddr()
        {
           return base.WriteAndReadByte(EXTIO2_ADDRESS_REG);
        }

        public void SetLEDColor(byte pin, uint color)
        {
            pin &= 0x07;    // valid only for 8 pins
            byte[] data = new byte[3];
            data[0] = (byte)((color >> 16) & 0xff);
            data[1] = (byte)((color >> 8) & 0xff);
            data[2] = (byte)(color & 0xff);
            byte reg = (byte)(pin * 3 + EXTIO2_RGB_24B_REG);
            base.WriteBytes(reg, data);
        }

        public uint GetLEDColor(byte pin)
        {
            pin &= 0x07;    // valid only for 8 pins
            byte[] data = new byte[3];
            byte reg = (byte)(pin * 3 + EXTIO2_RGB_24B_REG);
            base.WriteAndReadBytes(reg, data);
            return (uint)((data[0] << 16) + (data[1] << 8) + data[2]);
        }

        public void SetDigitalOutput(byte pin, byte state)
        {
            pin &= 0x07;    // valid only for 8 pins
            byte reg = (byte)(pin + EXTIO2_OUTPUT_CTL_REG);
            base.WriteBytes(reg, new byte[] { state });
        }

        public void SetAllDigitalOutputs(byte pins)
        {
            base.WriteBytes(EXTIO2_OUTPUTS_CTL_REG, new byte[] { pins });
        }

        public byte GetDigitalInput(byte pin)
        {
            pin &= 0x07;    // valid only for 8 pins
            return base.WriteAndReadByte((byte)(pin + EXTIO2_DIGITAL_INPUT_REG));
        }

        public byte GetAllDigitalInputs()
        {
            return base.WriteAndReadByte(EXTIO2_DIGITAL_INPUTS_REG);
        }

        public ushort GetAnalogInput(byte pin, ExtioAnalogReadMode bit = ExtioAnalogReadMode._8bit)
        {
            pin &= 0x07;    // valid only for 8 pins
            if (bit == ExtioAnalogReadMode._8bit)
            {
                byte reg = (byte)(pin + EXTIO2_ANALOG_INPUT_8B_REG);
                return (ushort)base.WriteAndReadByte(reg);
            }
            else
            {
                byte[] data = new byte[2];
                byte reg = (byte)(pin * 2 + EXTIO2_ANALOG_INPUT_12B_REG);
                base.WriteAndReadBytes(reg, data);
                return (ushort)((data[1] << 8) | data[0]);
            }
        }

        public bool SetServoAngle(byte pin, byte angle)
        {
            // Implementation for setting servo angle
            return true;
        }

        public bool SetServoPulse(byte pin, ushort pulse)
        {
            // Implementation for setting servo pulse
            return true;
        }

        public bool SetPwmDutyCycle(byte pin, byte duty)
        {
            // Implementation for setting PWM duty cycle
            return true;
        }

        public bool SetPwmFrequency(byte pin, byte freq)
        {
            // Implementation for setting PWM frequency
            return true;
        }
    }
}
