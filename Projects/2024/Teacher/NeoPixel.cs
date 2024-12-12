
// code snippet for NeoPixel (LED)
// nuget assembly: nanoFramework.Iot.Device.Ws28xx.Esp32
// nuget assembly: nanoFramework.System.Device.Gpio
//
// based on https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Ws28xx.Esp32


using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;

// Configure the count of pixels
const int Count = 1; 
// Adjust the pin number, where is the NeoPixel connected. 
const int PinNeo = 20;               // NanoC6: 20
// Adjust the pin number, where is the NeoPixel Power connected. 
const int PinNeoPower = 19;          // NanoC6: 19
// Create controller for Input/Output pins
static GpioController ioctrl = new GpioController();


//
// usage of Ws2808 driver    
//
// set the power on NeoPixel
if (PinNeoPower >= 0) ioctrl.OpenPin(PinNeoPower, PinMode.Output).Write(PinValue.High);
//
Ws28xx neo = new Ws2808(PinNeo, Count);
//neo.Image.SetPixel(0, 0, Color.Blue);
neo.Image.SetPixel(0, 0, 10, 0, 0);
neo.Update();
Thread.Sleep(1000);
neo.Image.SetPixel(0, 0, Color.Black);
neo.Update();


// more funny with NeoPixel
//Rainbow(neo, Count);




// function 
static void Rainbow(Ws28xx neo, int count, int iterations = 10)
{
     BitmapImage img = neo.Image;
     for (var i = 0; i < 255 * iterations; i++)
     {
          for (var j = 0; j < count; j++)
          {
              img.SetPixel(j, 0, Wheel((i + j) & 255));
          }
          neo.Update();
      }
  }




