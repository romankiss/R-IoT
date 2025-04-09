// usage of the driver for IO Extensions via I2C Bus

static M5_EXTIO2 sensorIOExt = null;



I2cDevice i2c_ioext = new(new I2cConnectionSettings(1, M5_EXTIO2.DefaultI2cAddress)); // Grove connector
res = i2c_ioext.WriteByte(0x07);
if (res.Status == I2cTransferStatus.FullTransfer)
{
    sensorIOExt = new M5_EXTIO2(i2c_ioext, ExtioIoMode.DigitalInputMode);    // all pins are inputs
    sensorIOExt.SetPinMode(3, ExtioIoMode.DigitalOutputMode);                // reconfigure the pin#3 for output
    sensorIOExt.SetPinMode(0, ExtioIoMode.RgbLedMode);                       // reconfigure the pin#0 for RGBLed (Neopixel)
    sensorIOExt.SetLEDColor(0, (uint)Color.Blue.ToArgb());                   // set Neopixel#0 to Blue color
}




private static void FireTelemetryData()
{
   // ...

   if (sensorIOExt != null)
   {
       // control output#3 based on the input#1
       sensorIOExt?.SetDigitalOutput(3, sensorIOExt.GetDigitalInput(1));
   }
  
   // ...
}




