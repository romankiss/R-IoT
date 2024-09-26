using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using Iot.Device.Ssd13xx;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;
using Iot.Device.Vl53L0X;

namespace S3
{
    public class Program
    {
        private static M5AtomicMotion motor;
        private static GpioButton button;
        public static void Main()
        {
            button = new GpioButton(41);
            Configuration.SetPinFunction(38, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(39, DeviceFunction.I2C2_CLOCK);
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);
            I2cConnectionSettings setting = new I2cConnectionSettings(2, 0x38);
            I2cConnectionSettings encSet = new I2cConnectionSettings(1, 0x40);
            I2cConnectionSettings distance = new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress);
            I2cDevice i2c2 = I2cDevice.Create(distance);
            I2cDevice i2c = I2cDevice.Create(setting);
            motor = M5AtomicMotion.Create(i2c);

           

            var sensorToF = new Vl53L0X(i2c2, 1000);
            if (sensorToF != null)
            {
                sensorToF.HighResolution = true;
                sensorToF.Precision = Precision.ShortRange;
                sensorToF.MeasurementMode = MeasurementMode.Continuous;
                Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
            }

            I2cDevice i2c_oled72x40 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C, I2cBusSpeed.StandardMode));
            var res = i2c_oled72x40.WriteByte(0x07);
            var display = new Ssd1306(i2c_oled72x40);
            ;
            int dis = sensorToF.Distance;

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                display = new Ssd1306(i2c_oled72x40) { Width = 99, Height = 64, Font = new Sinclair8x8(), Orientation = Iot.Device.Ssd13xx.DisplayOrientation.Landscape180 };
                display.ClearScreen();
                display.Write(0, 1, sensorToF.Distance.ToString(), 1);
                display.Display();
            }

      

            
            while (1 == 1) {
            if (sensorToF.Distance > 50)
            {
                motor.SetServoAngle(0, 0);
                motor.SetServoAngle(2, 180);
            }
            else
            {
                motor.SetServoAngle(0, 90);
                motor.SetServoAngle(2, 90);
            }

            int a = dis - sensorToF.Distance;
            if (a < 0)
            {
                a = a * -1;
            }

            if (a > 5){
            display.ClearScreen();
            display.Write(0, 1, sensorToF.Distance.ToString(), 1);
            display.Display();
            dis = sensorToF.Distance;
            }

            } 
            
            Thread.Sleep(Timeout.Infinite);

        }
    }
}
