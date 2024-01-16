using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bh1750fvi;


using Iot.Device.Ssd13xx;
using System.Diagnostics;
using System;



// https://docs.nanoframework.net/devicesdetails/Mfrc522/README.html
// https://github.com/nanoframework/nanoFramework.IoT.Device/blob/develop/devices/Mfrc522/samples/Program.cs

namespace NFAppAtomLite_Testing
{
    public class Program
    {
        public static void Main()
        {




            Configuration.SetPinFunction(21, DeviceFunction.I2C1_CLOCK);   // Grove connector
            Configuration.SetPinFunction(25, DeviceFunction.I2C1_DATA);    // Grove connector
            //
            I2cDevice i2c_oled128x64 = I2cDevice.Create(new I2cConnectionSettings(1, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2c_oled128x64);
            display.Orientation = DisplayOrientation.Landscape180;
            display.ClearScreen();
            display.Font = new Sinclair8x8();



            Configuration.SetPinFunction(22, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C2_CLOCK);

            I2cConnectionSettings settings = new I2cConnectionSettings(busId: 2, (int)I2cAddress.AddPinLow);
            I2cDevice device = I2cDevice.Create(settings);
            Bh1750fvi sensor = new Bh1750fvi(device);

            while (true)
            {

                double illuminance = sensor.Illuminance.Value;





                // read illuminance(Lux)
                Debug.WriteLine(illuminance.ToString());
                display.ClearScreen();
                display.Write(0, 0, illuminance.ToString());
                display.Display();



                Thread.Sleep(200);
            }
        }

    }
}

