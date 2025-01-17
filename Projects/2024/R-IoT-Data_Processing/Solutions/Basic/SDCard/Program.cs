//SPI SD Card without card detection
using nanoFramework.System.IO.FileSystem;
using nanoFramework.System.IO;
using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using System.Device.Spi;
using System.IO;
using System.Text;

namespace SD_card
{
    public class Program
    {
        static SDCard mycard0;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework - SD card Mount example");
            const string DirectoryPath = "D:\\";
            const string telemetrydataFilePath = DirectoryPath + "Nano.json";


            Configuration.SetPinFunction(14, DeviceFunction.SPI2_MISO);
            Configuration.SetPinFunction(12, DeviceFunction.SPI2_CLOCK);
            Configuration.SetPinFunction(13, DeviceFunction.SPI2_MOSI);
            mycard0 = new SDCard(new SDCardSpiParameters { spiBus = 2 , chipSelectPin = 11 });


            Debug.WriteLine("SDcard inited");

            

            WebStorage SD = new WebStorage(telemetrydataFilePath, mycard0); 


            SD.WriteData("telemetrydataFilePath");
            SD.ReadData();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
