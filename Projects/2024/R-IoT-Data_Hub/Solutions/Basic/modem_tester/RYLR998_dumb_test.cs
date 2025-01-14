/*This is a very bad attemp to measure, how many Bytes can the modem send during one socond.
Recommended to be not used (this code). Commiting just for archivation.*/

using System;
using System.Device.Gpio;
using System.Diagnostics;//also for stopwatch
using System.Threading;
using nanoFramework.Hardware.Esp32; // nugget used to config the pin functions (as tx, rx, I2C_CLK...)

namespace TroughputTester
{
    public class Program
    {
        private static bool _shouldStop = false;
        private static RYLR998 lora;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            #region LoRa
            // Use COM2/COM3 to avoid conflict with the USB Boot port
            Configuration.SetPinFunction(2, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(1, DeviceFunction.COM2_RX);
            lora = RYLR998.Create("COM2", 123);
            if (lora != null)
            {
                Debug.WriteLine("LoRa configed successfully!");

                // Start the watchdog in a separate thread

                Thread watchdogThread = new Thread(Watchdog);

                watchdogThread.Start();


                try

                {

                    // Start the recursive function

                    RecursiveFunction(1);

                }

                catch (Exception ex)

                {

                    Console.WriteLine($"Exception: {ex.Message}");

                }

                finally

                {

                    // Stop the watchdog

                    _shouldStop = true;

                    watchdogThread.Join(); // Wait for the watchdog to finish

                }
                // Create a new Stopwatch instance
                /* Stopwatch stopwatch = new Stopwatch();
                 var payload = "";
                 Debug.WriteLine($"S. f: {Stopwatch.Frequency}");


                 for (int i = 1; i <= 100; i++)
                 {
                     payload += "a";//make the pld one Byte larger
                     stopwatch.Reset();
                     // Start the stopwatch
                     stopwatch.Start();

                     lora.SendAsync(0, payload);

                     // Stop the stopwatch
                     stopwatch.Stop();


                     // Get the elapsed time in ticks
                     long elapsedTicks = stopwatch.ElapsedTicks;


                     // Calculate elapsed time in nanoseconds

                     // The frequency of the stopwatch is typically 1 tick = 100 nanoseconds

                     //long elapsedNanoseconds = elapsedTicks * (1000000000 / Stopwatch.Frequency);
                     float elapsedSeconds = 1 / (float)(elapsedTicks * Math.Pow(10, 14));

                     // Output the elapsed time

                     Console.WriteLine($"Payload with {i} Bytes took: {elapsedSeconds/1000} miliseconds");

                 }*/

            }
        #endregion

        Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void Watchdog()

        {

            // Wait for 1 second before stopping the recursive function

            Thread.Sleep(5000);

            _shouldStop = true; // Set the flag to stop the recursive function

        }


        private static void RecursiveFunction(int count)

        {

            // Check if we should stop

            if (_shouldStop)

            {

                Console.WriteLine("Stopping recursive function.");
                Debug.WriteLine("SO we send one byte (a) " + (count-1) + " times.");

                return; // Exit the function if the flag is set

            }


            // Simulate some work

            //Console.WriteLine($"Count: {count}");
            lora.SendAsync(0, "a");
            


            // Recursive call

            RecursiveFunction(count + 1);

        }
    }
}
