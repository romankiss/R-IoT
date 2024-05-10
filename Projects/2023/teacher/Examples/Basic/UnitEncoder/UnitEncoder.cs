// written by Roman Kiss, March 21st, 2024,
// version: 1.0.0.0 March 21,2024
//
// based on: https://github.com/m5stack/M5Unit-Encoder/blob/master/src/Unit_Encoder.h
// tool for converting to c#: https://www.codeconvert.ai/c++-to-csharp-converter
//
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace NFAppAtomLite_Testing
{
    public class EncoderEventArgs : EventArgs
    {
        public short NewValue { get; set; }
        public short OldValue { get; set; }
        public bool ButtonStatus { get; set; }
    }

    public delegate void OnEncoderEvent(object source, EncoderEventArgs e);

    public class Encoder : I2cDeviceBase
    {
        //const byte ENCODER_CHANGE_MODE = 0x00;
        const byte ENCODER_REG = 0x10;
        const byte BUTTON_REG = 0x20;
        const byte RGB_LED_REG = 0x30;
        //const byte ENCODER_RESET_COUNTER = 0x40;
        //
        public const byte DefaultI2cAddress = 0x40;
        public event OnEncoderEvent OnChangeValue = null;

        public Encoder(I2cDevice i2cDevice, int pollingTimeInMilliseconds = 100) : base(i2cDevice)
        {
            var task = new Thread(() =>
            {
                BlinkLED(0, 5, 5, 5);  
                Debug.WriteLine($">> Encoder Worker is ready for status polling.");
                int sleepTime = pollingTimeInMilliseconds;
                short lastValue = 0;
                bool lastButton = false;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(sleepTime);
                        if (OnChangeValue != null)
                        {
                            short currentValue = GetEncoderValue();
                            bool buttonStatus = GetButtonStatus();
                            if (lastValue != currentValue || lastButton != buttonStatus || buttonStatus)
                            {
                                OnChangeValue.Invoke(this, new EncoderEventArgs() { OldValue = lastValue, NewValue = currentValue, ButtonStatus = buttonStatus });
                                lastValue = currentValue;
                                lastButton = buttonStatus;
                                sleepTime = pollingTimeInMilliseconds;
                            }
                            else
                            {
                                sleepTime = pollingTimeInMilliseconds * 5;
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            })
            { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }
        public static Encoder Create(int busId = 1, bool bScan = true, int pollingTimeInMilliseconds = 100)
        {
            var i2cDevice = I2cDeviceBase.IsDeviceConnected(DefaultI2cAddress, busId, bScan);
            return i2cDevice != null ? new Encoder(i2cDevice, pollingTimeInMilliseconds) : null;
        }
        public static Encoder Create(I2cDevice i2cDevice, int pollingTimeInMilliseconds = 100)
        {
            return I2cDeviceBase.IsDeviceConnected(i2cDevice) != null ? new Encoder(i2cDevice, pollingTimeInMilliseconds) : null;
        }

        public short GetEncoderValue()
        {
            byte[] buffer = new byte[2];
            ReadBytes(ENCODER_REG, buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public bool GetButtonStatus()
        {
            byte[] buffer = new byte[2];
            ReadBytes(BUTTON_REG, buffer);
            return buffer[0] == 0;
        }

        public void SetLEDColor(byte index, byte r = 0, byte g = 0, byte b = 0)
        {
            WriteBytes(RGB_LED_REG, new byte[] { index, r, g, b });
        }
        public void SetLEDColor(byte index, uint color)
        {
            byte[] buffer = BitConverter.GetBytes(color);
            WriteBytes(0x30, new byte[] { index, buffer[0], buffer[1], buffer[2] });
        }

        public void BlinkLED(byte index, byte r, byte g, byte b, int timeInMS = 100, int period = 2)
        {
            for (int i = 0; i < period; i++)
            {
                SetLEDColor(index, r, g, b);
                Thread.Sleep(timeInMS);
                SetLEDColor(index);
                if(period > 1)
                    Thread.Sleep(timeInMS);
            }
        }
    }
}
