using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Hcsr501;
using System.Device.Gpio;


namespace PIR
{

    // https://docs.nanoframework.net/devicesdetails/Mfrc522/README.html
    // https://github.com/nanoframework/nanoFramework.IoT.Device/blob/develop/devices/Mfrc522/samples/Program.cs

    public class Program
    {
        static Hcsr501 sensorPIR = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

        #region PIR
            sensorPIR = new Hcsr501(25);    //ATOMLITE HAT - 25 (I2C OUT)
            sensorPIR.Hcsr501ValueChanged += Motion;

            Thread.Sleep(Timeout.Infinite);

        }

        private static void Motion(object sender, EventArgs e)
        {
            Debug.WriteLine("Pohyb!");
        }
        #endregion
    }
}
