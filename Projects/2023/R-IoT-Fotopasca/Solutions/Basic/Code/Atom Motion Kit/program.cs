using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;//this library needs to be added,, probably in the same dir as this file
using Iot.Device.Button;


namespace Atom_Motion_Demo
{
    public class Program
    {
        public static int servo_angle = 25;
        public static M5AtomicMotion servo = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            try
            {
                // Configure the I2C GPIOs used for the bus
                Configuration.SetPinFunction(38, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(39, DeviceFunction.I2C1_CLOCK);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }

            //Create the button object
            GpioButton button = new GpioButton(buttonPin: 41, debounceTime: TimeSpan.FromMilliseconds(200));


            // Initialize I2C device on the default address of 0x38
            I2cDevice i2c = new I2cDevice(new I2cConnectionSettings(1, M5AtomicMotion.DefaultI2cAddress));
            servo = M5AtomicMotion.Create(i2c);



            //set the angle of the servo on the 4th (0,1,2,3) channel/port to 25degrees
            servo.SetServoAngle(3, (byte)servo_angle);
            Debug.WriteLine($"angle: {servo_angle}");

            //bind the handler with the event watcher of button
            button.Press += (sender, e) => handle_button();




            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        public static void handle_button()
        {
            //get the current angle of the servo
            servo_angle = servo.GetServoAngle(3);
            if (servo_angle >= 85)
            {
                //if the clamp is already closed, open it again
                servo_angle = 25;
            }
            else
            {
                //if the clamp is not fully closed yet close it a bit
                servo_angle += 5;
            }
            servo.SetServoAngle(3, (byte)servo_angle);
            Debug.WriteLine($"angle: {servo_angle}");
        }
    }
}
