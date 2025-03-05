
static ToFSense sensorTOFSense = null;



// Waveshare ToF Sensor
i2c_tof = new(new I2cConnectionSettings(1, ToFSense.DefaultI2cAddress)); // Grove connector
res = i2c_tof.WriteByte(0x07);
if (res.Status == I2cTransferStatus.FullTransfer)
{
    sensorTOFSense = new ToFSense(i2c_tof);
    Debug.WriteLine($"sensorTOF: {sensorTOFSense.Distance} mm");
}
