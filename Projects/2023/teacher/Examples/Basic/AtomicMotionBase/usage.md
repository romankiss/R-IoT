
<h4>Create a driver class for Atomic Motion Base station</h4>

`var servo = M5AtomicMotion.Create(busId:2);`
<br></br>

<h4>Example of the tilting servo SG90 connected on the servoChannel=1</h4> 
Note, that the each pressed button, the Tilt is moved for 5 degrees in the range of 0 ~ 180 angles and then it is returned back to the 0 position. 
<br></br>

    private static void Button_Press(object sender, EventArgs e)
    {
      Debug.WriteLine($"\r\nButton has been pressed!");

      if(servo != null)
      {
          var angle = servo.GetServoAngle(1);
          Debug.WriteLine($"ServoAngle motion = {angle}");
          if (angle > 180)
              servo.SetServoAngle(1, 0);
          else
              servo.SetServoAngle(1, (byte)(angle + 5));
      }
    }

<br></br>
<h4>Demo: Distance sensor on the Tilt frame driven by AtomicMotion Base with AtomicLite controller</h4>

https://github.com/romankiss/R-IoT/assets/30365471/06e289ce-b46f-4c6f-b29f-37b52f296eb6




            #region ToF 
            I2cDevice i2c_tof = I2cDevice.Create(new I2cConnectionSettings(1, Vl53L0X.DefaultI2cAddress));
            res = i2c_oled72x40.WriteByte(0x07);
            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                sensorToF = new Vl53L0X(i2c_tof, 1000);
                if (sensorToF != null)
                {
                    sensorToF.HighResolution = true;
                    sensorToF.Precision = Precision.ShortRange;
                    sensorToF.MeasurementMode = MeasurementMode.Continuous;
                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                    display.WriteText(0, 0, $"ToF:{sensorToF.Distance} ").Show();
                }
            }
            #endregion

