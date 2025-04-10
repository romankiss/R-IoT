using System;
using nanoFramework.UI;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using System.Drawing;

namespace ATOMLiteS3R_DisplayDemo
{
    public class Program
    {
        public static void Main()
        {
            // Configure SPI pins for ATOMLiteS3R
            Configuration.SetPinFunction(12, DeviceFunction.SPI2_MISO);  // MISO (if used)
            Configuration.SetPinFunction(13, DeviceFunction.SPI2_MOSI);  // MOSI
            Configuration.SetPinFunction(14, DeviceFunction.SPI2_CLOCK);  // CLK

            ushort screenWidth = 128;
            ushort screenHeight = 128;

            var screen = DisplayControl.Initialize(
                new SpiConfiguration(
                    2,                          // SPI bus ID (2 for ESP32)
                    chipselect: 15,             // Chip Select pin (adjust per your device)
                    dataCommand: 21,            // DC pin
                    reset: 0,                   // Reset pin (0 if not used)
                    backLight: 0                 // Backlight pin (0 if not used)
                ),
                new ScreenConfiguration(0, 0, screenWidth, screenHeight)
            );

            // Create a Bitmap for drawing
            var bitmap = new Bitmap(screenWidth, screenHeight);
            var font = Primitives.SmallFont; // Use built-in small font

            // Clear screen (black)
            bitmap.Clear();

            // Draw text
            bitmap.DrawText("Hello", font, Color.White, 10, 10);
            bitmap.DrawText("nanoFramework!", font, Color.Yellow, 10, 20);

            // Flush to display
            bitmap.Flush();

            Thread.Sleep(Timeout.Infinite); // Keep running
        }
    }
}