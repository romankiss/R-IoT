
static ToFSense sensorTOF = null;



// Waveshare ToF Sensor
I2cDevice i2c_tof = new(new I2cConnectionSettings(1, ToFSense.DefaultI2cAddress)); // Grove connector
var res = i2c_tof.WriteByte(0x07);
if (res.Status == I2cTransferStatus.FullTransfer)
{
    sensorTOF = new ToFSense(i2c_tof);
    Debug.WriteLine($"sensorTOF: {sensorTOF.Distance} mm");
}
